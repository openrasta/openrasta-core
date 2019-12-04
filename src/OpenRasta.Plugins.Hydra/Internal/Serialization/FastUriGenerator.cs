using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class FastUriGenerator
  {
    readonly IUriResolver _uris;
    readonly Dictionary<Type, Func<object, string>> _generators;

    public FastUriGenerator(IMetaModelRepository repository, IUriResolver uris)
    {
      _uris = uris;
      var resourceGenerators = (from model in repository.ResourceRegistrations
        where model.ResourceType != null
        let generators = model.Uris.Where(uri => uri.Properties.ContainsKey("compiled")).ToList()
        where generators.Count == 1
        select new {model.ResourceType, generator = (Func<object, string>) (generators[0].Properties["compiled"])})
        .ToList();
      _generators =
        resourceGenerators
        .ToDictionary(x => x.ResourceType, x => x.generator);
    }

    public string CreateUri<T>(Uri baseUri)
    {
      if (_generators.TryGetValue(typeof(T), out var generator))
      {
        return baseUri + generator(null).Substring(1);
      }

      return _uris.CreateUriFor(baseUri, typeof(T)).ToString();
    }

    public string CreateUri(object instance, Uri baseUri)
    {
      return _generators.TryGetValue(instance.GetType(), out var generator)
        ? $"{baseUri}{generator(instance).Substring(1)}"
        : _uris.CreateFrom(instance, baseUri).ToString();
    }
  }
}