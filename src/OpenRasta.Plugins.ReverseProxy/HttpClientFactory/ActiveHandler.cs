using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRasta.Plugins.ReverseProxy.HttpClientFactory
{
  public class ActiveHandler : DelegatingHandler
  {
    Action<ActiveHandler> _evict;
    readonly Func<ActiveHandler, bool> _externalEviction;
    public int Index { get; }
    long _activationTime = 0;
    int _evicted = 0;
    Timer _evictionCheckTimer;
    readonly long _clientLeaseTime;
    readonly Stopwatch _started;

    public ActiveHandler(
      int index,
      HttpMessageHandler innerHandler,
      TimeSpan clientLeaseTime,
      Action<ActiveHandler> evict,
      Func<ActiveHandler, bool> externalEviction = null) : base(innerHandler)
    {
      _evict = evict ?? throw new ArgumentNullException(nameof(evict));
      _externalEviction = externalEviction;
      Index = index;
      var periodicity = clientLeaseTime < TimeSpan.FromSeconds(10) ? clientLeaseTime : TimeSpan.FromSeconds(10);
      _evictionCheckTimer = new Timer(CheckEviction, null, periodicity, periodicity);
      _clientLeaseTime = clientLeaseTime.Ticks;
      _started = Stopwatch.StartNew();
    }

    void CheckEviction(object state)
    {
      if (_started.ElapsedTicks > _clientLeaseTime || _externalEviction?.Invoke(this) == true)
        Evict();
    }

    public bool IsActive => DateTimeOffset.UtcNow.Ticks >= _activationTime;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      try
      {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable && response.Headers.RetryAfter != null)
        {
          var delay =
            response.Headers.RetryAfter.Delta
            ?? (response.Headers.RetryAfter.Date - (response.Headers.Date ?? DateTimeOffset.UtcNow));
          _activationTime = DateTimeOffset.UtcNow.Ticks + delay?.Ticks ?? 0;
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
      if (Interlocked.Exchange(ref _evicted, 1) != 0) return;

      _evictionCheckTimer.Dispose();
      _evictionCheckTimer = null;
      _evict(this);
      _evict = null;
    }
  }
}