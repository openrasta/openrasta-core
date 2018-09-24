using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Serialization;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Schemas;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet
{
  public class JsonNetApiDocumentationMetaModelHandler : IMetaModelHandler
  {
    public void PreProcess(IMetaModelRepository repository)
    {
    }

    public void Process(IMetaModelRepository repository)
    {
      var resources = from model in repository.ResourceRegistrations
        where model.ResourceType != null && model.ResourceType.GetInterfaces().Any(x => x == typeof(IEnumerable)) == false
        let hydraModel = model.Hydra()
        where hydraModel.Vocabulary != null &&
              hydraModel.Vocabulary != Vocabularies.Hydra
        select (model: model, hydraModel: hydraModel);

      foreach (var resource in resources)
      {
        resource.hydraModel.ApiClass = GetApiClass(resource.model, resource.hydraModel);
      }
    }

    Class GetApiClass(ResourceModel model, HydraResourceModel hydraModel)
    {
      var resourceType = model.ResourceType ?? throw new ArgumentException("Resource model is not a type");

      var contract = (JsonObjectContract) new CamelCasePropertyNamesContractResolver().ResolveContract(resourceType);

      var vocabPrefix = hydraModel.Vocabulary.DefaultPrefix;
      var className = model.ResourceType.Name;
      var identifier = vocabPrefix != null ? $"{vocabPrefix}:{className}" : className;

      return new Class
      {
        Identifier = identifier,
        SupportedProperties = contract.Properties.Select(p => new SupportedProperty
          {Property = new Rdf.Property {Identifier = $"{identifier}/{p.PropertyName}", Range = TryGetRange(p.PropertyType)}}).ToList(),
        SupportedOperations = hydraModel.SupportedOperations
      };
    }

    string TryGetRange(Type propertyType)
    {
      if (propertyType == typeof(string))
      {
        return "xsd:string";
      }

      return null;
    }
  }
}