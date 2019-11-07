using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public static partial class Hydra
  {
    public class Collection<T>
    {
      public Collection(IEnumerable<T> enumerable)
      {
        Member = enumerable.ToArray();
        Manages = new CollectionManages
        {
          Object = typeof(T).Name
        };
      }

      public CollectionManages Manages { get; }
      public T[] Member { get; set; }
      public int? TotalItems => Member?.Length;
    }
  }
}