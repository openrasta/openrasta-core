using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Configuration.MetaModel;
using Utf8Json;

namespace Tests.Plugins.Hydra.Utf8Json
{
  public static class TypeMethods
  {
    static readonly MethodInfo ResolverGetFormatterMethodInfo =
      typeof(CustomResolver).GetMethod(nameof(IJsonFormatterResolver.GetFormatter));

//    public static IEnumerable<Expression> WriteObjectPropertyValue(
//      ParameterExpression jsonWriter,
//      ParameterExpression resource,
//      PropertyInfo property
//    )
//    {
//      // var val = resource.property;
//      var variable = Variable(property.PropertyType);
//      var member = Expression.Property()
//      yield return 
//    }
    public static void Resource(
      ParameterExpression jsonWriter,
      ResourceModel model,
      Expression resource,
      Action<ParameterExpression> variable,
      Action<Expression> statement)
    {
      var type = model.ResourceType.Name;
      foreach (var exp in BeginObjectWithContextAndType(jsonWriter, type))
        statement(exp);

      var resolver = Expression.Variable(typeof(CustomResolver), "resolver");
      variable(resolver);
      statement(Expression.Assign(resolver, Expression.New(typeof(CustomResolver))));

      foreach (var pi in model.ResourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
        WriteResourceProperty(jsonWriter, resource, variable, statement, pi, resolver);
      }

      statement(JsonWriterMethods.WriteEndObject(jsonWriter));
    }

    static void WriteResourceProperty(ParameterExpression jsonWriter, Expression resource,
      Action<ParameterExpression> variable,
      Action<Expression> statement, PropertyInfo pi, ParameterExpression jsonFormatterResolver)
    {
      statement(JsonWriterMethods.WriteValueSeparator(jsonWriter));
      statement(WritePropertyName(jsonWriter, ToCamelCase(pi.Name)));


      var propertyType = pi.PropertyType;

      var (formatterInstance, serializeMethod) = GetFormatter(variable, statement, jsonFormatterResolver, propertyType);

      var propertyAccess = Expression.MakeMemberAccess(resource, pi);
      var serializeFormatter =
        Expression.Call(formatterInstance, serializeMethod, jsonWriter, propertyAccess, jsonFormatterResolver);
      statement(serializeFormatter);
    }

    static (ParameterExpression formatterInstance, MethodInfo serializeMethod) GetFormatter(
      Action<ParameterExpression> variable, Action<Expression> statement, ParameterExpression jsonFormatterResolver,
      Type propertyType)
    {
      var resolverGetFormatter = ResolverGetFormatterMethodInfo.MakeGenericMethod(propertyType);
      var jsonFormatterType = typeof(IJsonFormatter<>).MakeGenericType(propertyType);
      var serializeMethod = jsonFormatterType.GetMethod("Serialize",
        new[] {typeof(JsonWriter).MakeByRefType(), propertyType, typeof(IJsonFormatterResolver)});

      var formatterInstance = Expression.Variable(jsonFormatterType);
      statement(Expression.Assign(formatterInstance, Expression.Call(jsonFormatterResolver, resolverGetFormatter)));
      variable(formatterInstance);
      return (formatterInstance, serializeMethod);
    }

    static string ToCamelCase(string piName)
    {
      return char.ToLowerInvariant(piName[0]) + piName.Substring(1);
    }

    public static IEnumerable<Expression> BeginObjectWithContextAndType(ParameterExpression jsonWriter,
      string type)
    {
      yield return JsonWriterMethods.WriteRaw(jsonWriter, Nodes.BeginObjectContextComa);
      yield return WritePropertyName(jsonWriter, "@type");
      yield return WriteString(jsonWriter, type);
    }

    public static ParameterExpression Var<T>(string name)
    {
      return Expression.Variable(typeof(T), name);
    }

    public static Expression WritePropertyName(ParameterExpression jsonWriter, string propertyName)
    {
      var writer = new JsonWriter();
      writer.WritePropertyName(propertyName);
      var bytes = writer.ToUtf8ByteArray();
      return JsonWriterMethods.WriteRaw(jsonWriter, bytes);
    }

    public static Expression WriteString(ParameterExpression jsonWriter, string value)
    {
      var writer = new JsonWriter();
      writer.WriteString(value);
      var bytes = writer.ToUtf8ByteArray();
      return JsonWriterMethods.WriteRaw(jsonWriter, bytes);
    }
  }
}