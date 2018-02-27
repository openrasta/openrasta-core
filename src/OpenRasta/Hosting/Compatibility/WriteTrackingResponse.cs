using System.Threading;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Compatibility
{
  public class WriteTrackingResponse : IResponse
  {
    readonly IResponse _response;

    public WriteTrackingResponse(IResponse response)
    {
      _response = response;
      Entity = new WriteTrackingEntity(response.Entity, this);
    }

    public IHttpEntity Entity { get; }
    public HttpHeaderDictionary Headers => _response.Headers;

    public bool HeadersSent => _response.HeadersSent;

    public int StatusCode
    {
      get => _response.StatusCode;
      set => _response.StatusCode = value;
    }

    int _headersWritten = 0;

    public void WriteHeaders()
    {
      if (Interlocked.CompareExchange(ref _headersWritten, 1, 0) == 0)
        _response.WriteHeaders();
    }
  }
}