using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Schemas;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Internal
{
  public class ApiDocumentationHandler
  {
    readonly IMetaModelRepository _models;
    HydraOptions _opt;

    public ApiDocumentationHandler(IMetaModelRepository models)
    {
      _models = models;
      _opt = models.CustomRegistrations.OfType<HydraOptions>().Single();
    }

    public ApiDocumentation Get()
    {
      return new ApiDocumentation()
      {
        SupportedClasses = GenerateClasses()
      };
    }

    Class[] GenerateClasses()
    {
      return (
        from model in _models.ResourceRegistrations
        where model.ResourceType != null
        let hydraModel = model.Hydra()
        where hydraModel.Vocabulary != null &&
              hydraModel.Vocabulary != Vocabularies.Hydra &&
              model.Uris.All(uri => uri.Hydra().CollectionItemType == null)
        select GenerateClass(model, hydraModel)
      ).ToArray();
    }

    Class GenerateClass(ResourceModel model, HydraResourceModel hydraModel)
    {
      var resourceType = model.ResourceType ?? throw new ArgumentException("Resource model is not a type");

      var contract = (JsonObjectContract)new CamelCasePropertyNamesContractResolver().ResolveContract(resourceType);
      
      var vocabPrefix = hydraModel.Vocabulary.DefaultPrefix;
      var className = model.ResourceType.Name;
      return new Class
      {
        Identifier = $"{vocabPrefix}:{className}",
        SupportedProperties = contract.Properties.Select(p=>new SupportedProperty(p)).ToList()
      };
    }
  }
}