using OpenRasta.Data;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Diagnostics.Trace
{
  public class TraceHandler
  {
    readonly IRequest _request;

    public TraceHandler(IRequest request)
    {
      _request = request;
    }

    public IRequest Trace(Any _)
    {
      return _request;
    }
  }
}