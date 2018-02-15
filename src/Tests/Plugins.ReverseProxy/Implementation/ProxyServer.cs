using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Hosting.AspNetCore;
using OpenRasta.Web;
using Tests.Plugins.ReverseProxy.Implementation;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ProxyServer
  {
    string _from;
    string _to;
    Action<ReverseProxyOptions> _fromOptions;
    Action<ReverseProxyOptions> _toOptions;
    readonly List<Func<RequestBuilder, RequestBuilder>> _requests = new List<Func<RequestBuilder, RequestBuilder>>();
    Func<ICommunicationContext, string> _handler;
    TestServer _toServer;

    public ProxyServer FromServer(string from, Action<ReverseProxyOptions> options = null)
    {
      _from = from;
      _fromOptions = options;
      return this;
    }

    public ProxyServer ToServer(string to, Func<ICommunicationContext, string> handler = null, Action<ReverseProxyOptions> options = null)
    {
      _to = to;
      _toOptions = options;
      _handler = handler;
      return this;
    }

    public ProxyServer AddHeader(string headerName, string headerValue)
    {
      _requests.Add(req => req.AddHeader(headerName, headerValue));
      return this;
    }

    public async Task<HttpResponseMessage> GetAsync(string uri)
    {
      using (var fromServer = CreateFromServer())
      using (var toServer = CreateToServer())
      {
        var request = fromServer.CreateRequest(uri);
        _requests.ForEach(req => request = req(request));
        return await request.GetAsync();
      }
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
                        _to,
                        options,
                        _handler));
              }));
    }

    TestServer CreateFromServer()
    {
      var options = new ReverseProxyOptions
      {
          HttpMessageHandler = () => _toServer.CreateHandler()
      };
      _fromOptions?.Invoke(options);

      return new TestServer(
          new WebHostBuilder()
              .Configure(app =>
              {
                app.UseOpenRasta(
                    new ProxyApiFrom(_from, _to, options));
              }));
    }
  }
}