using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Internal;
using OpenRasta.Plugins.Hydra.Internal.Serialization;
using OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet;
using OpenRasta.Plugins.Hydra.Schemas;

namespace OpenRasta.Plugins.Hydra
{
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
        .ResourcesOfType<HydraCore.EntryPoint>()
        .Vocabulary(Vocabularies.Hydra)
        .AtUri(r => "/")
        .HandledBy<EntryPointHandler>();

      has
        .ResourcesOfType<HydraCore.Context>()
        .Vocabulary(Vocabularies.Hydra)
        .AtUri(r => "/.hydra/context.jsonld")
        .HandledBy<ContextHandler>();

      has
        .ResourcesOfType<HydraCore.ApiDocumentation>()
        .Vocabulary(Vocabularies.Hydra)
        .AtUri(r => "/.hydra/documentation.jsonld")
        .HandledBy<ApiDocumentationHandler>();

      has.ResourcesOfType<HydraCore.Collection>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<HydraCore.CollectionWithIdentifier>()
        .Vocabulary(Vocabularies.Hydra)
        .Type("hydra:Collection");

      has.ResourcesOfType<HydraCore.Class>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<HydraCore.SupportedProperty>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<HydraCore.IriTemplate>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<HydraCore.IriTemplateMapping>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<HydraCore.Operation>().Vocabulary(Vocabularies.Hydra);
      has.ResourcesOfType<Rdf.Property>().Vocabulary(Vocabularies.Rdf);

      if (opts.Serializer != null)
        uses.Dependency(opts.Serializer);

      uses.Dependency(ctx => ctx.Singleton<FastUriGenerator>());


      return uses;
    }

    public static IResourceDefinition<T> SupportedOperation<T>(
      this IResourceDefinition<T> resource,
      HydraCore.Operation operation)
    {
      resource.Resource.Hydra().SupportedOperations.Add(operation);
      return resource;
    }

    public static IResourceDefinition Vocabulary(this IResourceDefinition resource, Vocabulary vocab)
    {
      resource.Resource.Hydra().Vocabulary = vocab;
      return resource;
    }

    public static IResourceDefinition<T> Vocabulary<T>(this IResourceDefinition<T> resource, Vocabulary vocab)
    {
      resource.Resource.Hydra().Vocabulary = vocab;
      return resource;
    }

    public static IResourceDefinition<T> Type<T>(this IResourceDefinition<T> resource, Func<T, string> type)
    {
      resource.Resource.Hydra().JsonLdTypeFunc =
        obj => type((T) (obj ?? throw new NullReferenceException("current node was null")));
      return resource;
    }

    public static IResourceDefinition Type(this IResourceDefinition resource, string type)
    {
      resource.Resource.Hydra().JsonLdType = type;
      return resource;
    }


    public static IResourceDefinition<T> Link<T>(this IResourceDefinition<T> resource, SubLink link)
    {
      resource.Resource.Links.Add(new ResourceLinkModel
      {
        Relationship = link.Rel,
        Uri = link.Uri,
        CombinationType = ResourceLinkCombination.SubResource,
        Type = link.Type
      });
      return resource;
    }

    public static IUriDefinition<T> EntryPointCollection<T>(
      this IUriDefinition<T> resource,
      Action<CollectionEntryPointOptions> options = null)
    {
      var ienum = typeof(T).GetInterfaces()
        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToList();

      if (ienum.Count != 1)
        throw new ArgumentException("The resource definition implements multiple IEnumerable interfaces");

      var uriModel = resource.Uri.Hydra();

      var opts = new CollectionEntryPointOptions();
      options?.Invoke(opts);

      uriModel.ResourceType = typeof(T);
      uriModel.EntryPointUri = opts.Uri ?? resource.Uri.Uri;
      uriModel.SearchTemplate = opts.Search;
      return resource;
    }

    public static HydraResourceModel Hydra(this ResourceModel model)
    {
      return model.Properties.GetOrAdd("openrasta.Hydra.ResourceModel", () => new HydraResourceModel(model));
    }

    public static HydraUriModel Hydra(this UriModel model)
    {
      return model.Properties.GetOrAdd("openrasta.Hydra.UriModel", () => new HydraUriModel(model));
    }
  }
}