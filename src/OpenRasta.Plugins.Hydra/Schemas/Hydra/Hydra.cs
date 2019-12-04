using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public static partial class Hydra
  {
    public class Collection<T>  : Collection
    {
      public Collection()
      {
      }

      public Collection(IEnumerable<T> enumerable)
      {
        Member = enumerable.ToArray();
      }

      public T[] Member { get; set; }
      public override int? TotalItems => Member?.Length;

    }
  }
}