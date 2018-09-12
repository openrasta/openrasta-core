using System.Collections.Generic;

namespace OpenRastaDemo
{
  public class HydraHandler
  {
    public List<HydraRootResponse> Get()
    {
      return DemoHydraResponse.LargeJson;
    }
  }
}