using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy.LoadBalancingHttpClientFactory
{
  public class locking_to_dns_record
  {
    ResponseCapturingHttpMessageHandler simulator;
    HttpClient client;

    public locking_to_dns_record()
    {
      Task<IPAddress> resolver(string domainName) => Task.FromResult(IPAddress.Parse("127.0.0.1"));

      simulator = new ResponseCapturingHttpMessageHandler();
      var delegateHandler = new LockToIPAddress(simulator, resolver);
      client = new HttpClient(delegateHandler);
    }
    [Fact]
    public async Task request_uri_is_rewritten()
    {
      await client.GetAsync("http://localhost/path");
      simulator.Request.RequestUri.ToString().ShouldBe("http://127.0.0.1/path");
    }
    [Fact]
    public async Task host_header_is_original_host()
    {
      await client.GetAsync("http://localhost/path");
      simulator.Request.Headers.Host.ShouldBe("localhost");
    }

    [Fact]
    public async Task only_first_host_gets_rewritten()
    {
      await client.GetAsync("http://localhost/path");
      simulator.Request.RequestUri.ToString().ShouldBe("http://127.0.0.1/path");
      
      await client.GetAsync("http://google.com/path");
      simulator.Request.RequestUri.ToString().ShouldBe("http://google.com/path");
    }

    [Fact]
    public async Task ip_address_is_not_rewritten()
    {
      await client.GetAsync("http://192.168.0.1/path");
      simulator.Request.RequestUri.ToString().ShouldBe("http://192.168.0.1/path");
      simulator.Request.Headers.Host.ShouldBe("192.168.0.1");
    }
  }

  class ResponseCapturingHttpMessageHandler : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      Request = request;
      return Task.FromResult(new HttpResponseMessage());
    }

    public HttpRequestMessage Request { get; set; }
  }
}