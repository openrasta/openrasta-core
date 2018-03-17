using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Tests.Hosting.Owin;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
#if NETCOREAPP2_0
  public class corefx_repro
  {
    [Fact(Skip = "#136 – fails, disable until https://github.com/dotnet/corefx/issues/28156 is resolved")]
    public async Task maybe_itll_fail_i_dont_even()
    {
      var toServer =
        new WebHostBuilder()
          .UseKestrel()
          .UseUrls("http://127.0.0.1:0")
          .Configure(app =>
          {
            app.Run(async context =>
            {
              await context.Response.WriteAsync("hello");
            });
          })
          .Build();
      await toServer.StartAsync();
      var toPort = toServer.Port();
      
      var rpClient = new ReproReverseProxy();
      var fromServer = new WebHostBuilder()
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
      await fromServer.StartAsync();

      var client = new HttpClient();
      var response = await client.GetAsync($"http://127.0.0.1:{fromServer.Port()}");
      response.EnsureSuccessStatusCode();
      (await response.Content.ReadAsStringAsync()).ShouldBe("hello");
    }
  }

  public class ReproCodec
  {
    public static async Task Write(HttpContext context, ReverseProxyResponse proxyResponse)
    {
      context.Response.StatusCode = proxyResponse.StatusCode;

      if (proxyResponse.ResponseMessage != null)
      {
        await proxyResponse.ResponseMessage.Content.CopyToAsync(context.Response.Body);
      }

      await context.Response.Body.FlushAsync();
    }
  }

  public class ReproReverseProxy
  {
    public async Task<ReverseProxyResponse> Send(HttpContext context, string target)
    {
      var httpClient = new HttpClient(new HttpClientHandler());
      var requestMessage = new HttpRequestMessage
      {
        Method = new HttpMethod(context.Request.Method),
        Content = new StreamContent(context.Request.Body)
      };

      CopyHeaders(context, requestMessage);

      requestMessage.RequestUri = new Uri(target);
      try
      {
        var responseMessage = await httpClient.SendAsync(
          requestMessage,
          HttpCompletionOption.ResponseHeadersRead
        );
        return new ReverseProxyResponse(requestMessage, responseMessage);
      }
      catch (TaskCanceledException e)
      {
        return new ReverseProxyResponse(requestMessage, error: e, statusCode: 504);
      }
    }

    static void CopyHeaders(HttpContext context, HttpRequestMessage request)
    {
      foreach (var header in context.Request.Headers)
      {
        if (header.Key.Equals("host", StringComparison.OrdinalIgnoreCase)) continue;
        if (header.Key.StartsWith("Content-"))
        {
          request.Content.Headers.Add(header.Key, header.Value.ToList());
        }
        else
          request.Headers.Add(header.Key, header.Value.ToList());
      }
    }
  }
#endif
}