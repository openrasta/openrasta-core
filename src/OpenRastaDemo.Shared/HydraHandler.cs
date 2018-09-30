using System.Collections.Generic;

namespace OpenRastaDemo
{
  public class HydraHandler<T>
  {
    readonly List<T> _data;

    public HydraHandler(List<T> data)
    {
      _data = data;
    }
    public List<T> Get()
    {
      return _data;
    }
  }
}