using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy.HttpClientFactory;
using OpenRasta.Plugins.ReverseProxy.HttpMessageHandlers;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy.LoadBalancingHttpClientFactory
{
  public class getting_clients
  {
    [Fact]
    public void creates_as_many_handlers_as_requested_count()
    {
      var createdHandlers = new List<HttpMessageHandler>();

      HttpMessageHandler createHandler()
      {
        var handler =
          new DelegateHandler((message, token) => Task.FromResult(new HttpResponseMessage()));
        createdHandlers.Add(handler);
        return handler;
      }

      var factory = new RoundRobinHttpClientFactory(5, createHandler, Timeout.InfiniteTimeSpan);

      var clients = Enumerable.Range(0, 10).Select(i => factory.GetClient()).ToList();
      clients.Distinct().Count().ShouldBe(10);
      createdHandlers.Count.ShouldBe(5);
    }
  }
}