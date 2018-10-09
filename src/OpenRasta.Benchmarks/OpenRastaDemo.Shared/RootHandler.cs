using System.Collections.Generic;

namespace OpenRastaDemo.Shared
{
  public class RootHandler
  {
    public IEnumerable<RootResponse> Get()
    {
      return DemoJsonResponse.LargeJson;
    }
  }
}