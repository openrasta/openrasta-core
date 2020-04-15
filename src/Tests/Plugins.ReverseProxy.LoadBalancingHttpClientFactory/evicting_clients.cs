using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
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
    public async Task evicts_503_handlers_temporarily()
    {
      var activeHandler = new DelegateHandler((message, token) => Task.FromResult(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent("active")
      }));
      int timedOut = 0;
      var retryHandler = new DelegateHandler((message, token) =>
      {
        if (Interlocked.Exchange(ref timedOut, 1) == 0)
          return Task.FromResult(new HttpResponseMessage()
          {
            StatusCode = HttpStatusCode.ServiceUnavailable,
            Headers = {RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(1))}
          });
        return Task.FromResult(new HttpResponseMessage()
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent("reactivated")
        });
      });
      
      var handlerIndex = 0;
      var factory = new RoundRobinHttpClientFactory(2, () => handlerIndex++ *2 == 0 ? activeHandler : retryHandler, TimeSpan.FromMinutes(2));

      var responsesBefore = await execute();
      await Task.Delay(TimeSpan.FromSeconds(1));
      var responsesAfter = await execute();

      responsesBefore.Count(r => r.response.StatusCode == HttpStatusCode.ServiceUnavailable).ShouldBe(1);
      responsesBefore.Any(r => r.content == "reactivated").ShouldBeFalse();

      responsesAfter.All(r => r.response.StatusCode == HttpStatusCode.OK).ShouldBeTrue();
      responsesAfter.Count(r => r.content == "reactivated").ShouldBe(5);
      responsesAfter.Count(r => r.content == "active").ShouldBe(5);
      
      
      async Task<List<(HttpResponseMessage response, string content)>> execute()
      {
        List<(HttpResponseMessage httpResponseMessage, string content)> response = new List<(HttpResponseMessage httpResponseMessage, string content)>();
        for (int i = 0; i < 10; i++)
        {
          using (var client = factory.GetClient())
          {
            var httpResponseMessage = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://nowhere.example/path"));
            var content = httpResponseMessage.Content == null
              ? null
              : await httpResponseMessage.Content?.ReadAsStringAsync();
            response.Add((httpResponseMessage,content));
          }
        }

        return response;
      }
      
      
    }

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

      var factory = new RoundRobinHttpClientFactory(1, createHandler, TimeSpan.FromSeconds(1));

      var clients = Enumerable.Range(0, 10).Select(i => factory.GetClient()).ToList();
      clients.Count.ShouldBe(10);
      createdHandlers.Count.ShouldBe(1);

      await Task.Delay(TimeSpan.FromSeconds(5));
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