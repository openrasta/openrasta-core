using System.Net;
using System.Threading.Tasks;
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
      using (var response = await new ProxyServer().FromServer("/proxy")
        .ToServer("/proxied")
        .AddHeader("Accept", "application/vnd.example")
        .GetAsync("http://localhost/proxy"))
      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable);
      }
    }
  }
}