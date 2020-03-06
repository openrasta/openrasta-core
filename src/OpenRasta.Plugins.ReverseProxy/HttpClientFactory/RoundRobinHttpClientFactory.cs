using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRasta.Plugins.ReverseProxy.HttpClientFactory
{
  public class RoundRobinHttpClientFactory
  {
    readonly int _count;
    readonly Func<int, HttpMessageHandler> _handlerFactory;
    readonly TimeSpan _clientLeaseTime;
    readonly Func<ActiveHandler, bool> _shouldEvict;
    readonly ConcurrentDictionary<int, ActiveHandler> _handlers;
    readonly ConcurrentQueue<EvictedHandler> _evictedHandlers;

    int _index;

    public RoundRobinHttpClientFactory(
      int count,
      Func<HttpMessageHandler> handlerFactory,
      TimeSpan? clientLeaseTime = null,
      Func<ActiveHandler, bool> shouldEvict = null)
    {
      _count = count;
      _handlerFactory = position => handlerFactory();
      _clientLeaseTime = clientLeaseTime ?? TimeSpan.FromSeconds(2);
      _shouldEvict = shouldEvict;
      _handlers = new ConcurrentDictionary<int, ActiveHandler>();
      _evictedHandlers = new ConcurrentQueue<EvictedHandler>();
      _cleanupTimer = new Timer(state => Cleanup(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public HttpClient GetClient(string host = null)
    {
      ActiveHandler handler;
      int tries = 0;
      do
      {
        if (tries++ > 50) throw new HttpClientFactoryException("All HTTP clients are busy, try again later");
        var idx = Interlocked.Increment(ref _index);
        var clientKey = idx > 0 ? idx % _count : idx * -1 % _count;
        handler = _handlers.GetOrAdd(clientKey, CreateActiveHandler);
      } while (handler.IsActive == false);

      return new HttpClient(handler, disposeHandler: false);
    }

    ActiveHandler CreateActiveHandler(int position)
    {
      return new ActiveHandler(position, _handlerFactory(position), _clientLeaseTime, EvictHandler, _shouldEvict);
    }

    void EvictHandler(ActiveHandler handler)
    {
      if (!_handlers.TryRemove(handler.Index, out _)) throw new InvalidOperationException("Could not remove handler");

      _evictedHandlers.Enqueue(new EvictedHandler(handler));
      ScheduleCleanup();
    }

    void Cleanup()
    {
      _cleanupScheduled = 0;

      var unready = new List<EvictedHandler>();

      while (_evictedHandlers.TryDequeue(out var handler))
      {
        if (handler.IsDisposable) handler.Dispose();
        else unready.Add(handler);
      }

      if (!unready.Any()) return;

      foreach (var handler in unready)
        _evictedHandlers.Enqueue(handler);

      ScheduleCleanup();
    }

    readonly Timer _cleanupTimer;
    int _cleanupScheduled = 0;

    void ScheduleCleanup()
    {
      if (Interlocked.Exchange(ref _cleanupScheduled, 1) == 0)
        _cleanupTimer.Change(TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
    }
  }
}