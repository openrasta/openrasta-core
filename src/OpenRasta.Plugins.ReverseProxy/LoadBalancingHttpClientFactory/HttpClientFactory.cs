using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace OpenRasta.Plugins.ReverseProxy.LoadBalancingHttpClientFactory
{
  public class HttpClientFactory
  {
    readonly int _count;
    readonly Func<HttpMessageHandler> _handlerFactory;
    readonly TimeSpan _clientLeaseTime;
    readonly ConcurrentDictionary<int, ActiveHandler> _handlers;
    readonly ConcurrentQueue<EvictedHandler> _evictedHandlers;

    int _index;

    public HttpClientFactory(int count, Func<HttpMessageHandler> handlerFactory, TimeSpan clientLeaseTime)
    {
      _count = count;
      _handlerFactory = handlerFactory;
      _clientLeaseTime = clientLeaseTime;
      _handlers = new ConcurrentDictionary<int, ActiveHandler>();
      _evictedHandlers = new ConcurrentQueue<EvictedHandler>();
      _cleanupTimer = new Timer(state => Cleanup(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public HttpClient GetClient()
    {
      var idx = Interlocked.Increment(ref _index);
      var clientKey = idx > 0 ? idx % _count : idx * -1 % _count;

      return new HttpClient(_handlers.GetOrAdd(clientKey, CreateActiveHandler), disposeHandler: false);
    }

    ActiveHandler CreateActiveHandler(int position)
    {
      return new ActiveHandler(position, _handlerFactory(), _clientLeaseTime, EvictHandler);
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
        _cleanupTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
    }
  }

  class ActiveHandler : DelegatingHandler
  {
    Action<ActiveHandler> _evict;
    public int Index { get; }
    Timer _evictionTimer;

    public ActiveHandler(int index, HttpMessageHandler innerHandler, TimeSpan clientLeaseTime,
      Action<ActiveHandler> evict) : base(innerHandler)
    {
      _evict = evict;
      Index = index;
      _evictionTimer = new Timer(Evict, evict, clientLeaseTime, Timeout.InfiniteTimeSpan);
    }

    void Evict(object state)
    {
      _evictionTimer.Dispose();
      _evictionTimer = null;
      _evict(this);
      _evict = null;
    }
  }

  class EvictedHandler
  {
    readonly WeakReference _weakReference;

    public EvictedHandler(ActiveHandler handler)
    {
      _disposableHandler = handler.InnerHandler;
      _weakReference = new WeakReference(handler);
    }

    readonly HttpMessageHandler _disposableHandler;
    public bool IsDisposable => !_weakReference.IsAlive;

    public void Dispose()
    {
      _disposableHandler.Dispose();
    }
  }
}