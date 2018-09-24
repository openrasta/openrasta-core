using System.Collections.Generic;

namespace OpenRastaDemo
{
  public class RootHandler
  {
    public IEnumerable<RootResponse> Get()
    {
      return DemoJsonResponse.LargeJson;
    }
  }
}