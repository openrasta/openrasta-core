using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.DI;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Internal;
using OpenRasta.Plugins.Hydra.Internal.Serialization;
using OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8Json;
using OpenRasta.Plugins.Hydra.Schemas;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra
{
  public class HydraOperationOptions
  {
  }

  public static class ConfigurationExtensions
  {
    public static IUses Hydra(this IUses uses, Action<HydraOptions> hydraOptions = null)
    {
      var fluent = (IFluentTarget) uses;
      var has = (IHas) uses;

      var opts = new HydraOptions
      {
        Curies =
        {
          Vocabularies.Hydra,
          Vocabularies.SchemaDotOrg,
          Vocabularies.Rdf,
          Vocabularies.XmlSchema
        }
      };

      hydraOptions?.Invoke(opts);

      fluent.Repository.CustomRegistrations.Add(opts);

      has.ResourcesOfType<object>()
        .WithoutUri
        .TranscodedBy<JsonLdCodecWriter>().ForMediaType("application/ld+json")
        .And.TranscodedBy<JsonLdCodecReader>().ForMediaType("application/ld+json");

      has
        .ResourcesOfType<EntryPoint>()
        .Vocabulary(Vocabularies.Hydra)
        .AtUri("/")
        .HandledBy<EntryPointHandler>();

      has
        .ResourcesOfType<Context>()
        .Vocabulary(Vocabularies.Hydra)
        .AtUri("/.hydra/context.jsonld")
        .HandledBy<ContextHandler>();

      has
        .ResourcesOfType<ApiDocumentation>()
        .Vocabulary(Vocabularies.Hydra)
        .AtUri("/.hydra/documentation.jsonld")
        .HandledBy<ApiDocumentationHandler>();

      has.ResourcesOfType<Collection>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<Class>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<SupportedProperty>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<IriTemplate>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<IriTemplateMapping>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<Operation>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<Rdf.Property>().Vocabulary(Vocabularies.Rdf);

      if (opts.Utf8Json)
      {
        uses.CustomDependency<IMetaModelHandler, Utf8JsonMetaModelHandler>(DependencyLifetime.Transient);
      }
      else
      {
        uses.CustomDependency<IMetaModelHandler, JsonNetMetaModelHandler>(DependencyLifetime.Transient);
      }

      uses.CustomDependency<IMetaModelHandler, JsonNetApiDocumentationMetaModelHandler>(DependencyLifetime.Transient);

      return uses;
    }


    public static IResourceDefinition<T> SupportedOperation<T>(this IResourceDefinition<T> resource, Operation operation)
    {
      resource.Resource.Hydra().SupportedOperations.Add(operation);
      return resource;
    }

    public static IResourceDefinition<T> Vocabulary<T>(this IResourceDefinition<T> resource, Vocabulary vocab)
    {
      resource.Resource.Hydra().Vocabulary = vocab;
      return resource;
    }

    public static IUriDefinition<T> EntryPointCollection<T>(this IUriDefinition<T> resource, Action<CollectionEntryPointOptions> options = null)
    {
      var ienum = typeof(T).GetInterfaces()
        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToList();

      if (ienum.Count != 1)
        throw new ArgumentException("The resource definition implements multiple IEnumerable interfaces");

      var itemType = ienum[0].GenericTypeArguments[0];

      var uriModel = resource.Uri.Hydra();

      var opts = new CollectionEntryPointOptions();
      options?.Invoke(opts);

      uriModel.CollectionItemType = itemType;
      uriModel.ResourceType = typeof(T);
      uriModel.EntryPointUri = opts.Uri ?? resource.Uri.Uri;
      uriModel.SearchTemplate = opts.Search;
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

  public class CollectionEntryPointOptions
  {
    public string Uri { get; set; }
    public IriTemplate Search { get; set; }
  }

  public class HydraOptions
  {
    public IList<Vocabulary> Curies { get; } = new List<Vocabulary>();
    public Vocabulary Vocabulary { get; set; }
    public bool Utf8Json { get; set; }
  }
}