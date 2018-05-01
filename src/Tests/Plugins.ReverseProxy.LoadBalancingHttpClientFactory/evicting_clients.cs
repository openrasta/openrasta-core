using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy.HttpClientFactory;
using OpenRasta.Plugins.ReverseProxy.HttpMessageHandlers;
using Shouldly;
using Tests.Plugins.ReverseProxy.LoadBalancingHttpClientFactory.Infrastructure;
using Xunit;

namespace Tests.Plugins.ReverseProxy.LoadBalancingHttpClientFactory
{
  public class evicting_clients
  {
    [Fact]
    public async Task evicts_handlers_after_timeout()
    {
      var createdHandlers = new List<HttpMessageHandler>();

      HttpMessageHandler createHandler()
      {
        var handler =
          new DelegateHandler((message, token) => Task.FromResult(new HttpResponseMessage()));
        createdHandlers.Add(handler);
        return handler;
      }

      var factory = new RoundRobinHttpClientFactory(1, createHandler, TimeSpan.FromSeconds(5));

      var clients = Enumerable.Range(0, 10).Select(i => factory.GetClient()).ToList();
      clients.Count.ShouldBe(10);
      createdHandlers.Count.ShouldBe(1);

      await Task.Delay(TimeSpan.FromSeconds(6));
      clients = Enumerable.Range(0, 10).Select(i => factory.GetClient()).ToList();
      clients.Count.ShouldBe(10);
      createdHandlers.Count.ShouldBe(2);
    }

    [Fact]
    public async Task evicts_throwing_handlers()
    {
      var handler = new DisposeTrackingHandler(
        new DelegateHandler((message, token) => throw new HttpRequestException()));
      var factory = new RoundRobinHttpClientFactory(1, () => handler);

      await CreateAndDisposeClientWithGarbageCollection(factory, async client =>
      {
        try
        {
          await client.GetAsync("http://nowhere.example");
        }
        catch
        {
        }
      });

      WaitForDispose(handler);

      handler.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public async Task disposes_handlers_once_out_of_scope()
    {
      DisposeTrackingHandler handler = null;

      HttpMessageHandler createHandler()
      {
        return handler =
          new DisposeTrackingHandler(
            new DelegateHandler((message, token) => Task.FromResult(new HttpResponseMessage())));
      }

      var factory = new RoundRobinHttpClientFactory(1, createHandler, TimeSpan.FromMilliseconds(1));

      await CreateAndDisposeClientWithGarbageCollection(factory);

      WaitForDispose(handler, TimeSpan.FromSeconds(15));

      handler.IsDisposed.ShouldBeTrue();
    }

    static void WaitForDispose(DisposeTrackingHandler handler, TimeSpan? timeout = null)
    {
      timeout = timeout ?? TimeSpan.FromSeconds(20);

      var sw = Stopwatch.StartNew();
      do
      {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true, true);
      } while (!handler.IsDisposed && sw.Elapsed < timeout);
    }

    async Task CreateAndDisposeClientWithGarbageCollection(RoundRobinHttpClientFactory factory,
      Func<HttpClient, Task> clientInvoker = null)
    {
      var client = factory.GetClient();
      if (clientInvoker != null) await clientInvoker(client);
      client.Dispose();
    }
  }
}