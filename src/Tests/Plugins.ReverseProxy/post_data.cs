using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class post_data
  {
    [Fact]
    public async Task data_is_received()
    {
      using(var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", context => new StreamReader(context.Request.Entity.Stream, Encoding.UTF8).ReadToEndAsync())
        .AddHeader("Content-Type", MediaType.ApplicationXWwwFormUrlencoded.ToString())
        .PostAsync("http://localhost/proxy", "key=value"))

      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldBe("key=value");
      }
    }
  }
}