using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class RelUriRelPath
  {
    public OperationResult Get()
    {
      return new OperationResult.OK {RedirectLocation = new Uri("relPathResource", UriKind.Relative)};
    }

    public Task<OperationResult> GetAsync()
    {
      return Task.Run(() => Get());
    }
  }
}