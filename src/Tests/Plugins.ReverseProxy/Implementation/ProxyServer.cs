using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.DI;
using OpenRasta.Hosting.AspNetCore;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;
using Tests.Hosting.Owin;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxyServer
  {
    Func<int, string> _fromUri;
    Func<int, string> _toUri;

    Action<ReverseProxyOptions> _fromOptions;
    Action<ReverseProxyOptions> _toOptions;
    readonly List<Action<HttpRequestMessage>> _requests = new List<Action<HttpRequestMessage>>();
    Func<ICommunicationContext, Task<string>> _handler;
    TestServer _toServer;
    Func<(HttpClient client, IDisposable disposer)> _serverFactory;

    public ProxyServer()
    {
      _serverFactory = CreateTestServers;
    }

    public ProxyServer FromServer(string fromUri, Action<ReverseProxyOptions> options = null)
    {
      _fromUri = port => fromUri;
      _fromOptions = options;
      return this;
    }

    public ProxyServer FromServer(Func<int, string> fromUri, Action<ReverseProxyOptions> options = null)
    {
      _fromUri = fromUri;
      _fromOptions = options;
      return this;
    }

    public ProxyServer ToServer(string toUri, Func<ICommunicationContext, Task<string>> handler = null,
      Action<ReverseProxyOptions> options = null)
    {
      _toUri = port => toUri;
      _toOptions = options;
      _handler = handler;
      return this;
    }

    public ProxyServer ToServer(Func<int, string> toUri, Func<ICommunicationContext, Task<string>> handler = null,
      Action<ReverseProxyOptions> options = null)
    {
      _toUri = toUri;
      _toOptions = options;
      _handler = handler;
      return this;
    }

    public ProxyServer AddHeader(string headerName, string headerValue)
    {
      _requests.Add(req =>
      {
        if (req.Headers.TryAddWithoutValidation(headerName, headerValue)) return;
        if (req.Content == null)
          req.Content = new StreamContent(Stream.Null);
        if (!req.Content.Headers.TryAddWithoutValidation(headerName, headerValue))
          throw new ArgumentException("Invalid header name: " + headerName, nameof(headerName));
      });
      return this;
    }

    public async Task<ProxyResponse> PostAsync(string uri, string body)
    {
      // Force Content-Length due to httpclient on server not setting the header correctly
      _requests.Add(req => req.Content = new StringContent(body) {Headers = {ContentLength = body.Length}});
      return await SendAsync("POST", uri);
    }

    public ProxyServer Request(Action<HttpRequestMessage> modifier)
    {
      _requests.Add(modifier);
      return this;
    }

    public async Task<ProxyResponse> GetAsync(string uri)
    {
      return await SendAsync("GET", uri);
    }

    public class ProxyResponse : IDisposable
    {
      public HttpResponseMessage Message { get; }
      readonly IDisposable _disposer;

      public ProxyResponse(HttpResponseMessage message, IDisposable disposer, string content)
      {
        Message = message;
        _disposer = disposer;
        Content = content;
      }

      public string Content { get; }

      public void Dispose()
      {
        _disposer?.Dispose();
      }
    }

    public ProxyServer UseKestrel()
    {
      _serverFactory = CreateKestrelServers;
      return this;
    }

    async Task<ProxyResponse> SendAsync(string method, string uri)
    {
      var (client, disposer) = CreateServersAndClient();

      var request = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), uri);
      _requests.ForEach(req => req(request));

      var response = await client.SendAsync(request);
      return new ProxyResponse(response, disposer, await response.Content.ReadAsStringAsync());
    }

    (HttpClient client, IDisposable disposer) CreateServersAndClient()
    {
      return _serverFactory();
    }

    (HttpClient client, IDisposable disposer) CreateKestrelServers()
    {
      var toServer = CreateKestrelToServer();
      var fromServer = CreateKestrelFromServer(toServer.port);
      var client = new HttpClient() {BaseAddress = new Uri($"http://127.0.0.1:{fromServer.port}")};
      var disposer = new ActionOnDispose(() =>
      {
        client.Dispose();
        fromServer.host.Dispose();
        toServer.host.Dispose();
      });
      return (client, disposer);
    }

    (IDisposable host, int port) CreateKestrelFromServer(int toPort)
    {
      var options = new ReverseProxyOptions();

      _fromOptions?.Invoke(options);
      var host =
        new WebHostBuilder()
          .Configure(app =>
          {
            app.UseOpenRasta(
              new ProxyApiFrom(_fromUri(80), _toUri(toPort), options));
          })
          .UseKestrel()
          .UseUrls("http://127.0.0.1:0")
          .Build();
      host.Start();

      return (host, host.Port());
    }

    (IDisposable host, int port) CreateKestrelToServer()
    {
      var options = new ReverseProxyOptions();
      _toOptions?.Invoke(options);
      var host =
        new WebHostBuilder()
          .UseKestrel()
          .UseUrls("http://127.0.0.1:0")
          .Configure(app =>
          {
            app.UseOpenRasta(
              new ProxyApiTo(
                _toUri(80),
                options,
                _handler));
          })
          .Build();
      host.Start();

      return (host, host.Port());
    }

    (HttpClient client, IDisposable disposer) CreateTestServers()
    {
      var fromServer = CreateFromServer(() => _toServer.CreateHandler());
      var toServer = CreateToServer();
      var client = fromServer.CreateClient();
      var disposer = new ActionOnDispose(() =>
      {
        fromServer.Dispose();
        toServer.Dispose();
      });
      return (client, disposer);
    }

    TestServer CreateToServer()
    {
      var options = new ReverseProxyOptions();
      _toOptions?.Invoke(options);
      return _toServer = new TestServer(
        new WebHostBuilder()
          .Configure(app =>
          {
            app.UseOpenRasta(
              new ProxyApiTo(
                _toUri(80),
                options,
                _handler));
          }));
    }

    TestServer CreateFromServer(Func<HttpMessageHandler> httpMessageHandler)
    {
      var options = new ReverseProxyOptions
      {
        HttpClient = {Handler = httpMessageHandler}
      };

      _fromOptions?.Invoke(options);

      return new TestServer(
        new WebHostBuilder()
          .Configure(app =>
          {
            app.UseOpenRasta(
              new ProxyApiFrom(_fromUri(80), _toUri(80), options));
          }));
    }
  }
}