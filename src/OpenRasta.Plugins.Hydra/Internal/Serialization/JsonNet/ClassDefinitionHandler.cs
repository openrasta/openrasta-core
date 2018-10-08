using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;

namespace OpenRasta.Plugins.Hydra.Internal
{
  public class ClassDefinitionHandler : IMetaModelHandler
  {
    public ClassDefinitionHandler()
    {
    }

    public void PreProcess(IMetaModelRepository repository)
    {
    }

    public void Process(IMetaModelRepository repository)
    {
      foreach (var resourceRegistration in repository.ResourceRegistrations.Where(x => x.ResourceType.Name == "Event"))
      {
        var classProperties = new List<ClassProperty>();
        var properties = resourceRegistration.ResourceType.GetProperties();

        foreach (var property in properties)
        {
          /*
           
           Check for json.net attributes first. If they use a mixture of attribute types then
           
              )
             (
            .-`-.
            :   :
            :TNT:
            :___:
           
           */

          if (property.CustomAttributes.Any(x => x.AttributeType == typeof(JsonIgnoreAttribute) || x.AttributeType == typeof(IgnoreDataMemberAttribute)))
          {
            continue;
          }

          string propertyName = string.Empty;

          if (property.CustomAttributes.Any(x => x.AttributeType == typeof(JsonPropertyAttribute)))
          {
            propertyName = property.CustomAttributes.Where(x => x.AttributeType == typeof(JsonPropertyAttribute)).Select(x => x.ConstructorArguments.FirstOrDefault().Value.ToString())
              .FirstOrDefault();
          }
          else if (property.CustomAttributes.Any(x => x.AttributeType == typeof(DataMemberAttribute)))
          {
            propertyName = property.CustomAttributes.Where(x => x.AttributeType == typeof(DataMemberAttribute)).SelectMany(x => x.NamedArguments).Where(c => c.MemberName == "Name")
              .Select(v => v.TypedValue.Value.ToString()).FirstOrDefault();
          }

          if (string.IsNullOrWhiteSpace(propertyName))
          {
            propertyName = MakeCamelCase(property.Name);
          }

          //Should we explicitly check for Collection type (i.e. HydraCollection)

          Type propertyType = null;

          if (property.PropertyType.GetGenericArguments().Any())
          {
            propertyType = property.PropertyType.GetGenericArguments()[0];
          }

          propertyType = propertyType ?? property.PropertyType;

          if (repository.ResourceRegistrations.Any(x => x.ResourceType == propertyType))
          {
            classProperties.Add(new ClassProperty(propertyName, repository.ResourceRegistrations.FirstOrDefault(x => x.ResourceType == propertyType)));
          }
          else
          {
            classProperties.Add(new ClassProperty(propertyName));
          }
        }

        //We need to store the Vocabulary but if we pass that to ClassDefinition we leak Hydra into OR.Core so do we pass a Dictionary<string,object> to ClassDefinition for plugin specific properties?

        resourceRegistration.ClassDefinition = new ClassDefinition(classProperties, resourceRegistration.ResourceType.Name, resourceRegistration);
      }
    }

    public static string MakeCamelCase(string name)
    {
      if (char.IsLower(name[0]))
      {
        return name;
      }

      return string.Concat(char.ToLowerInvariant(name[0]), name.Substring(1));
    }
  }
}