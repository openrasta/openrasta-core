using System.Collections.Generic;

namespace OpenRastaDemo
{
  public class HydraHandler
  {
    readonly List<HydraRootResponse> _data;

    public HydraHandler(List<HydraRootResponse> data)
    {
      _data = data;
    }
    public List<HydraRootResponse> Get()
    {
      return _data;
    }
  }
}