using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OpenRasta;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;
using Shouldly;
using Tests.Hosting.Owin;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Tests.Plugins.ReverseProxy
{
#if NETCOREAPP2_0
  public class get_returning_200_on_kestrel
  {
    [Fact(Skip = "#136 – fails, disable until https://github.com/dotnet/corefx/issues/28156 is resolved")]
    public async Task response_status_is_correct()
    {
      using (var response = await new ProxyServer()
        .FromServer(port => $"http://localhost:{port}/proxy")
        .ToServer(port => $"http://localhost:{port}/proxied")
        .UseKestrel()
        .GetAsync("proxy"))
      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldEndWith("/proxied");
      }
    }
  }

  public class corefx_repro
  {
    [Fact]
    public async Task maybe_itll_fail_i_dont_even()
    {
      var toServer =
        new WebHostBuilder()
          .UseKestrel()
          .UseUrls("http://127.0.0.1:0")
          .Configure(app => { app.Run(async context => await context.Response.WriteAsync("hello")); })
          .Build();
      toServer.Start();
      var toPort = toServer.Port();
      var rpClient = new ReproReverseProxy(new ReverseProxyOptions());
      var fromServer=new WebHostBuilder()
        .Configure(app =>
        {
          app.Run(async context =>
          {
            var targetResponse = await rpClient.Send(context, $"http://127.0.0.1:{toPort}");
            await ReproCodec.Write(context, targetResponse);
          });
        })
        .UseKestrel()
        .UseUrls("http://127.0.0.1:0")
        .Build();
      fromServer.Start();

      var client = new HttpClient();
      var response = await client.GetAsync($"http://127.0.0.1:{fromServer.Port()}");
      response.EnsureSuccessStatusCode();
    }
  }

  public class ReproCodec
  {
    public static async Task Write(HttpContext context, ReverseProxyResponse proxyResponse)
    {
      
      try
      {
        context.Response.StatusCode = proxyResponse.StatusCode;

        if (proxyResponse.ResponseMessage != null)
        {
//          foreach (var header in proxyResponse.ResponseMessage.Headers)
//            context.Response.Headers.Add(header.Key, header.Value.ToArray());

          await proxyResponse.ResponseMessage.Content.CopyToAsync(context.Response.Body);
          context.Response.Body.Flush();
        }
      }
      finally
      {
        proxyResponse.Dispose();
      }
    }
  }
  public class ReproReverseProxy
  {
    readonly ReverseProxyOptions _options;
    readonly Lazy<HttpClient> _httpClient;
    readonly TimeSpan _timeout;

    public ReproReverseProxy(ReverseProxyOptions options)
    {
      _options = options;
      _timeout = options.Timeout;
      _httpClient = new Lazy<HttpClient>(() => new HttpClient(_options.HttpMessageHandler())
        {
          Timeout = Timeout.InfiniteTimeSpan
        },
        LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<ReverseProxyResponse> Send(HttpContext context, string target)
    {
      var requestMessage = new HttpRequestMessage
      {
        Method = new HttpMethod(context.Request.Method),
        Content = new StreamContent(context.Request.Body)
      };

      CopyHeaders(context, requestMessage, _options.FrowardedHeaders.ConvertLegacyHeaders);

      var headers = requestMessage.Headers;
      var identifier = _options.Via.Pseudonym ?? "test";
      headers.Add("via", $"1.1 {identifier}");

      requestMessage.RequestUri = new Uri(target);
      var cts = new CancellationTokenSource();
      cts.CancelAfter(_timeout);
      var token = cts.Token;
      try
      {
        var responseMessage = await _httpClient.Value.SendAsync(
          requestMessage,
          HttpCompletionOption.ResponseHeadersRead,
          token
        );
        responseMessage.Headers.Via.Add(new ViaHeaderValue("1.1", identifier));
        return new ReverseProxyResponse(requestMessage, responseMessage);
      }
      catch (TaskCanceledException e) when (token.IsCancellationRequested || e.CancellationToken == token)
      {
        return new ReverseProxyResponse(requestMessage, error: e, statusCode: 504);
      }
    }

    static void CopyHeaders(HttpContext context, HttpRequestMessage request, bool convertLegacyHeaders)
    {
      foreach (var header in context.Request.Headers)
      {
        if (header.Key.Equals("host", StringComparison.OrdinalIgnoreCase)) continue;
        if (header.Key.StartsWith("Content-"))
          request.Content.Headers.Add(header.Key, header.Value.ToList());
        else
          request.Headers.Add(header.Key, header.Value.ToList());
      }

    }
  }
#endif
  
}