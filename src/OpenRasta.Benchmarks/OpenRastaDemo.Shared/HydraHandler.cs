using System.Collections.Generic;

namespace OpenRastaDemo.Shared
{
  public class HydraHandler
  {
    public List<HydraRootResponse> Get()
    {
      return DemoHydraResponse.LargeJson;
    }
  }
}