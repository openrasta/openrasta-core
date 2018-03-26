using System.Net;
using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_returning_200
  {
    [Fact]
    public async Task response_status_is_correct()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied")
        .GetAsync("http://localhost/proxy"))
      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldBe("http://destination.example/proxied");
      }
    }

    [Fact]
    public async Task proxying_request_has_empty_body()
    {

      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", async context => 
          $"{context.Request.Headers.ContentLength}|{context.Request.Headers["transfer-encoding"]}")
        .GetAsync("http://localhost/proxy"))
      {
        response.Content.ShouldBe("|");
        
      }
    }
  }
}