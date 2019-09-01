using System.Collections.Generic;

namespace OpenRastaDemo.Shared
{
  public class HydraRootHandler
  {
    readonly List<HydraRootResponse> _content;

    public HydraRootHandler(List<HydraRootResponse>  content)
    {
      _content = content;
    }

    public List<HydraRootResponse> Get()
    {
      return _content;
    }
  }

}