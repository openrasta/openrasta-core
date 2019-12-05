using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Configuration;

namespace OpenRasta.Plugins.Hydra.Schemas
{
  public static partial class HydraCore
  {
    public class Collection
    {
      public static CollectionWithIdentifier FromModel(Uri appBase, ResourceModel itemModel, HydraUriModel uriModel)
      {
        var collection = new CollectionWithIdentifier
        {
          Identifier = new Uri(appBase, new Uri(uriModel.EntryPointUri, UriKind.RelativeOrAbsolute)),
          Search = uriModel.SearchTemplate,
        };

        collection.Manages = new CollectionManages
          {Object = uriModel.Uri.ResourceModel.Hydra().Collection.ManagesRdfTypeName};
        return collection;
      }

      protected Collection()
      {
      }

      public IriTemplate Search { get; set; }

      public CollectionManages Manages { get; set; }
      public virtual int? TotalItems => null;
    }

    public class CollectionWithIdentifier : Collection
    {
      [JsonProperty("@id")]
      public Uri Identifier { get; set; }
    }

    public class Collection<T> : Collection
    {
      public Collection()
      {
      }

      public Collection(IEnumerable<T> enumerable, string managesRdf)
      {
        Member = enumerable.ToArray();
        Manages = new CollectionManages {Object = managesRdf};
      }

      public T[] Member { get; set; }
      public override int? TotalItems => Member?.Length;
    }
  }
}