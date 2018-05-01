using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy.HttpClientFactory;
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
        var handler = new NullHandler();
        createdHandlers.Add(handler);
        return handler;
      }

      var factory = new RoundRobinHttpClientFactory(5, createHandler, Timeout.InfiniteTimeSpan);

      var clients = Enumerable.Range(0, 10).Select(i => factory.GetClient()).ToList();
      clients.Distinct().Count().ShouldBe(10);
      createdHandlers.Count.ShouldBe(5);
    }

    [Fact]
    public async Task handler_is_evicted()
    {
      var createdHandlers = new List<HttpMessageHandler>();

      HttpMessageHandler createHandler()
      {
        var handler = new NullHandler();
        createdHandlers.Add(handler);
        return handler;
      }

      var factory = new RoundRobinHttpClientFactory(1, createHandler, TimeSpan.FromSeconds(5));

      var clients = Enumerable.Range(0, 10).Select(i => factory.GetClient()).ToList();;
      createdHandlers.Count.ShouldBe(1);
      
      await Task.Delay(TimeSpan.FromSeconds(6));
      clients = Enumerable.Range(0, 10).Select(i => factory.GetClient()).ToList();;
      createdHandlers.Count.ShouldBe(2);
    }
    [Fact]
    public async Task handler_is_disposed()
    {
      NullHandler handler = null;
      HttpMessageHandler createHandler()
      {
        return handler = new NullHandler();
      }

      var factory = new RoundRobinHttpClientFactory(1, createHandler, TimeSpan.FromMilliseconds(1));

      CreateAndDisposeClient(factory);
      
      var sw = Stopwatch.StartNew();

      TimeSpan timeout = TimeSpan.FromSeconds(15);
      do
      {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true, true);
      } while (!handler.IsDisposed && sw.Elapsed < timeout);
      
      handler.IsDisposed.ShouldBeTrue();
    }

    void CreateAndDisposeClient(RoundRobinHttpClientFactory factory)
    {
      var client = factory.GetClient();
      client.Dispose();
    }
  }

  class NullHandler : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      return Task.FromResult(new HttpResponseMessage());
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      IsDisposed = true;
    }

    public bool IsDisposed { get; set; }
  }
}