using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRasta.Plugins.ReverseProxy.HttpClientFactory
{
  public class RoundRobinHttpClientFactory
  {
    readonly int _count;
    readonly Func<HttpMessageHandler> _handlerFactory;
    readonly TimeSpan _clientLeaseTime;
    readonly ConcurrentDictionary<int, ActiveHandler> _handlers;
    readonly ConcurrentQueue<EvictedHandler> _evictedHandlers;

    int _index;

    public RoundRobinHttpClientFactory(int count, Func<HttpMessageHandler> handlerFactory,
      TimeSpan? clientLeaseTime = null)
    {
      _count = count;
      _handlerFactory = handlerFactory;
      _clientLeaseTime = clientLeaseTime ?? TimeSpan.FromMinutes(2);
      _handlers = new ConcurrentDictionary<int, ActiveHandler>();
      _evictedHandlers = new ConcurrentQueue<EvictedHandler>();
      _cleanupTimer = new Timer(state => Cleanup(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public HttpClient GetClient()
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

  public class HttpClientFactoryException : Exception
  {
    public HttpClientFactoryException(string message)
      : base(message)
    {
    }
  }

  class ActiveHandler : DelegatingHandler
  {
    Action<ActiveHandler> _evict;
    public int Index { get; }
    Timer _evictionTimer;
    long _activationTime;

    public ActiveHandler(int index, HttpMessageHandler innerHandler, TimeSpan clientLeaseTime,
      Action<ActiveHandler> evict) : base(innerHandler)
    {
      _evict = evict;
      Index = index;
      _evictionTimer = new Timer(Evict, evict, clientLeaseTime, Timeout.InfiniteTimeSpan);
      
    }

    public bool IsActive => DateTimeOffset.UtcNow.Ticks >= _activationTime;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      try
      {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
          var delay =
            response.Headers.RetryAfter.Delta
            ?? (response.Headers.RetryAfter.Date - (response.Headers.Date ?? DateTimeOffset.UtcNow))
            ?? TimeSpan.FromSeconds(5);
          _activationTime = DateTimeOffset.UtcNow.Ticks + delay.Ticks;
        }

        return response;
      }
      catch
      {
        Evict();
        throw;
      }
    }

    void Evict(object state = null)
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