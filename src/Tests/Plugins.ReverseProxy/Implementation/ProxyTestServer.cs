using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Hosting.AspNetCore;
using Tests.Plugins.ReverseProxy.Implementation;

namespace OpenRasta.Plugins.ReverseProxy
{
  public static class ProxyTestServer
  {
    public static TestServer Create(string from, string to)
    {
      TestServer testServer = null;
      testServer = new TestServer(
          new WebHostBuilder()
              .Configure(app => app.UseOpenRasta(
                  new ReverseProxyApi(
                      from,to,
                      new ReverseProxyOptions
                      {
                          HttpMessageHandler = () => testServer.CreateHandler()
                      }))));
      return testServer;
    }
  }
}