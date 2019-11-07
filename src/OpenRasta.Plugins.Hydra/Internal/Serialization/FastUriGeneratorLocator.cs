using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class FastUriGeneratorLocator
  {
    readonly IUriResolver _uris;
    readonly Dictionary<object, Func<object, string>> _generators;
    ILookup<object, Type> _typeToKey;

    public FastUriGeneratorLocator(IMetaModelRepository repository)
    {
      _generators =
        (from model in repository.ResourceRegistrations
          let generators = model.Uris.Where(uri => uri.Properties.ContainsKey("compiled")).ToList()
          where generators.Count == 1
          select new {model.ResourceKey, generator = (Func<object, string>) (generators[0].Properties["compiled"])})
        .ToDictionary(x => x.ResourceKey, x => x.generator);
      _typeToKey = repository.ResourceRegistrations
        .Where(r => r.ResourceType != null)
        .ToLookup(r => r.ResourceKey, r => r.ResourceType);
    }

//    public string CreateUri<T>(ResourceModel currentResource, Uri baseUri)
//    {
//      if (currentResource.ResourceType != typeof(T))
//      {
//        var resourceKeys = _typeToKey[typeof(T)];
////        if (resourceKeys.Count() == 1) return )_generators.
//      }
//    }
  }
}