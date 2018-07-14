using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Internal;
using OpenRasta.Plugins.Hydra.Internal.Serialization;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra
{
  public static class ConfigurationExtensions
  {
    public static IUses Hydra(this IUses uses)
    {
      var fluent = (IFluentTarget) uses;
      var has = (IHas) uses;

      has.ResourcesOfType<IJsonLdDocument>()
        .WithoutUri
        .TranscodedBy<JsonLdCodec>()
        .ForMediaType("application/ld+json");

      has
        .ResourcesOfType<EntryPoint>()
        .Vocabulary(Vocabularies.Hydra)
        .AtUri("/")
        .HandledBy<EntryPointHandler>();

      has
        .ResourcesOfType<Context>()
        .AtUri("/.hydra/context.jsonld")
        .HandledBy<RootContextHandler>();

      has
        .ResourcesOfType<Collection>()
        .Vocabulary(Vocabularies.Hydra);

      return uses;
    }

    public static IResourceDefinition<T> Vocabulary<T>(this IResourceDefinition<T> resource, Vocabulary vocab)
    {
      //var rd = (ResourceDefinition<T>) resource;
      resource.Resource.Hydra().Vocabulary = vocab;
      return resource;
    }

    public static IUriDefinition<T> Collection<T>(this IUriDefinition<T> resource)
    {
      var ienum = typeof(T).GetInterfaces()
        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToList();

      if (ienum.Count != 1)
        throw new ArgumentException("The resource definition implements multiple IEnumerable interfaces");

      var itemType = ienum[index: 0].GenericTypeArguments[0];

      var uriModel = resource.Uri.Hydra();

      uriModel.CollectionItemType = itemType;
      uriModel.ResourceType = typeof(T);
      return resource;
    }

    public static HydraResourceModel Hydra(this ResourceModel model)
    {
      return model.Properties.GetOrAdd<HydraResourceModel>("openrasta.Hydra.ResourceModel");
    }

    public static HydraUriModel Hydra(this UriModel model)
    {
      return model.Properties.GetOrAdd("openrasta.Hydra.UriModel", () => new HydraUriModel(model));
    }
  }
}