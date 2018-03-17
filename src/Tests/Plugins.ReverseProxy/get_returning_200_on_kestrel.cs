using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OpenRasta;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;
using Shouldly;
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
#endif
  
}