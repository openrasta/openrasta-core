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
    readonly Dictionary<(Type ResourceType, string resourceName), UriGenerator> _generators;

    public FastUriGenerator(IMetaModelRepository repository, IUriResolver uris)
    {
      _uris = uris;
      var resourceGenerators = (
          from models in repository.ResourceRegistrations.GroupBy(r=>r.ResourceType)
          where models.Key != null
          from model in models
          let resourceName = model.Name
          let generators = model.Uris.Where(uri => uri.Properties.ContainsKey("compiled")).ToList()
          where generators.Count == 1
          select new {model.ResourceType, resourceName, generator = (Func<object, string>) (generators[0].Properties["compiled"])})
        .ToList();
      _generators =
        resourceGenerators
          .ToDictionary(x => (x.ResourceType, x.resourceName),
            x => new UriGenerator(x.generator, x.resourceName));
    }

    class UriGenerator
    {
      public string ResourceName { get; }
      readonly Func<object, string> _generator;

      public UriGenerator(Func<object, string> generator, string resourceName)
      {
        ResourceName = resourceName;
        _generator = generator;
      }

      public string Create(object instance) => _generator(instance);
    }

    public string CreateUri<T>(Uri baseUri, string resourceName)
    {
      if (_generators.TryGetValue((typeof(T), resourceName), out var generator))
      {
        return baseUri + generator.Create(null).Substring(1);
      }

      return _uris.CreateUriFor(baseUri, typeof(T)).ToString();
    }

    public string CreateUri(object instance, Uri baseUri, string resourceName)
    {
      return _generators.TryGetValue((instance.GetType(), resourceName), out var generator)
        ? $"{baseUri}{generator.Create(instance).Substring(1)}"
        : _uris.CreateFrom(instance, baseUri).ToString();
    }
  }
}