using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet
{
  public class ClassDefinitionHandler//. : IMetaModelHandler
  {
    public static void CreateClassDefinitions(IMetaModelRepository repository)
    {
      foreach (var resourceRegistration in repository.ResourceRegistrations.Where(reg=>reg.ResourceType != null))
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

          if (property.CustomAttributes.Any(x => x.AttributeType.Name == "\JsonIgnoreAttribute" || x.AttributeType.Name == "IgnoreDataMemberAttribute"))
            continue;

          string propertyName = string.Empty;

          if (property.CustomAttributes.Any(x => x.AttributeType.Name == "JsonPropertyAttribute"))
            propertyName = property.CustomAttributes.Where(x => x.AttributeType.Name == "JsonPropertyAttribute")
              .Select(x => x.ConstructorArguments.FirstOrDefault(a => a.ArgumentType == typeof(string)).Value.ToString())
              .FirstOrDefault();
          else if (property.CustomAttributes.Any(x => x.AttributeType.Name == "DataMemberAttribute"))
            propertyName = property.CustomAttributes.Where(x => x.AttributeType.Name == "DataMemberAttribute").SelectMany(x => x.NamedArguments).Where(c => c.MemberName == "Name")
              .Select(v => v.TypedValue.Value.ToString()).FirstOrDefault();


          if (string.IsNullOrWhiteSpace(propertyName))
            propertyName = MakeCamelCase(property.Name);


          //Should we explicitly check for Collection type (i.e. HydraCollection)

          Type propertyType = null;

          if (property.PropertyType.GetGenericArguments().Any())
            propertyType = property.PropertyType.GetGenericArguments()[0];

          propertyType = propertyType ?? property.PropertyType;

          var inheritanceTree = propertyType.GetInheritanceHierarchy();
          var models = new List<ResourceModel>(); //Might want to use SortedList just be sure but should be ok here

          foreach (var type in inheritanceTree)
          {
            var result = repository.ResourceRegistrations.FirstOrDefault(x => x.ResourceType == type);
            if (result != null)
            {
              models.Add(result);
            }
          }

          classProperties.Add(new ClassProperty(propertyName, property.PropertyType, models, propertyType.GetRdfRange()));
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