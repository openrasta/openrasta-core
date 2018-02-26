using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_returning_406
  {
    [Fact]
    public async Task response_status_code_is_correct()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied")
          .AddHeader("Accept", "application/vnd.example")
          .GetAsync("http://localhost/proxy");
      response.response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable);
      response.dispose();
    }
  }
}