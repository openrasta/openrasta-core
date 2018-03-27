using System;
using System.Net;
using System.Net.Http;
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
    public async Task response_media_type_is_correct()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied")
        .GetAsync("http://localhost/proxy"))
      {
        response.Message.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
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
    [Fact]
    public async Task get_with_0_content_length_is_proxied_is_proxied_without_cl()
    {

      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", async context => $"{context.Request.Headers.ContentLength}")
        .Request(request=>request.Content = new ByteArrayContent(Array.Empty<byte>()) { Headers = { ContentLength = 0}})
        .GetAsync("http://localhost/proxy"))
      {
        response.Content.ShouldBe("");
      }
    }
  }
}