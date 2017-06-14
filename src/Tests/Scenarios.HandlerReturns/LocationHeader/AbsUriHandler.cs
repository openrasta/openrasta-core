using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class AbsUriHandler
  {
    public OperationResult Get()
    {
      return new OperationResult.OK {RedirectLocation = new Uri("http://localhost/absResource", UriKind.Absolute)};
    }

    public Task<OperationResult> GetAsync()
    {
      return Task.Run(() => Get());
    }
  }
}