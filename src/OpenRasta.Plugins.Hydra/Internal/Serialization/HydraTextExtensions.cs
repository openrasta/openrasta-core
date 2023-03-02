using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public static class HydraTextExtensions
  {
    public static string GetRdfRange(this Type type)
    {
      switch (type.Name)
      {
        case nameof(Int32):
          return "xsd:int";

        case nameof(String):
          return "xsd:string";

        case nameof(Boolean):
          return "xsd:boolean";

        case nameof(DateTime):
          return "xsd:datetime";

        case nameof(Decimal):
          return "xsd:decimal";

        case nameof(Double):
          return "xsd:double";

        case nameof(Uri):
          return "xsd:anyURI";

        default:
          return "xsd:string";
      }
    }

    public static IEnumerable<Type> GetInheritanceHierarchy(this Type type)
    {
      for (var current = type; current != null; current = current.BaseType)
      {
        yield return current;
      }
    }

    public static string ToCamelCase(this string piName) => char.IsLower(piName[0]) ? piName : char.ToLowerInvariant(piName[0]) + piName.Substring(1);

    public static bool IsNotIgnored(this PropertyInfo pi)
    {
      return pi.CustomAttributes.Any(a =>
               a.AttributeType.Name == "JsonIgnoreAttribute" 
               || a.AttributeType.Name == "IgnoreDataMemberAttribute"
               || a.AttributeType.Name == nameof(JsonLdIgnoreAttribute)) ==
             false;
    }

    internal static string UriStandardCombine(string current, Uri rel) => new Uri(new Uri(current), rel).ToString();

    internal static string UriSubResourceCombine(string current, Uri rel)
    {
      current = current[current.Length - 1] == '/' ? current : current + "/";
      return new Uri(new Uri(current), rel).ToString();
    }

    public static IEnumerable<Type> EnumerableItemTypes(this Type type)
    {
      return from i in type.GetInterfaces()
        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        select i.GetGenericArguments()[0];
    }

    public static string GetJsonLdTypeName(this ResourceModel model)
    {
      var hydraResourceModel = model.Hydra();
      var type = hydraResourceModel.JsonLdType ?? GetGenericToResourceTypeName(model.ResourceType);
      return (hydraResourceModel.Vocabulary?.DefaultPrefix == null
               ? String.Empty
               : $"{hydraResourceModel.Vocabulary.DefaultPrefix}:") +
             type;
    }

    static string GetGenericToResourceTypeName(Type modelResourceType)
    {

      return modelResourceType.IsGenericType == false
        ? modelResourceType.Name
        : string.Join("Of",
          modelResourceType.GetGenericArguments().Select(GetGenericToResourceTypeName)
            .Prepend(modelResourceType.Name.Substring(0,modelResourceType.Name.IndexOf('`'))));
    }

    public static string GetJsonPropertyName(this PropertyInfo pi)
    {
      return pi.CustomAttributes
               .Where(a => a.AttributeType.Name == "JsonPropertyAttribute")
               .SelectMany(a => a.ConstructorArguments)
               .Where(a => a.ArgumentType == typeof(string))
               .Select(a => (string) a.Value)
               .FirstOrDefault()
             ?? pi.CustomAttributes.Where(x => x.AttributeType.Name == "DataMemberAttribute")
               .SelectMany(x => x.NamedArguments)
               .Where(c => c.MemberName == "Name")
               .Select(v => v.TypedValue.Value.ToString()).FirstOrDefault()
             ?? pi.Name.ToCamelCase();
    }
  }
}