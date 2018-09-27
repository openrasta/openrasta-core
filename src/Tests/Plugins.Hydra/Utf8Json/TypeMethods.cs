using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Internal;
using Utf8Json;

namespace Tests.Plugins.Hydra.Utf8Json
{
  public static class TypeMethods
  {
    static readonly MethodInfo ResolverGetFormatterMethodInfo =
      typeof(CustomResolver).GetMethod(nameof(IJsonFormatterResolver.GetFormatter));

    static readonly PropertyInfo SerializationContextUriResolverPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.UriGenerator));
    static readonly PropertyInfo SerializationContextBaseUriPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.BaseUri));

    static readonly MethodInfo StringConcatTwoParamsMethodInfo =
      typeof(string).GetMethod(nameof(string.Concat), new[] {typeof(string), typeof(string)});

    public static Expression StringConcat(Expression first, Expression second)
      => Expression.Call(StringConcatTwoParamsMethodInfo, first, second);
    public static void Resource(
      ParameterExpression jsonWriter,
      ResourceModel model,
      Expression resource,
      Expression options,
      Action<ParameterExpression> variable,
      Action<Expression> statement)
    {
      var type = model.ResourceType.Name;
      var uriResolverFunc = Expression.MakeMemberAccess(options, SerializationContextUriResolverPropertyInfo);
      var uriResolver = Expression.Invoke(uriResolverFunc, resource);
      var contextUri = StringConcat(
        Expression.Call(Expression.MakeMemberAccess(options, SerializationContextBaseUriPropertyInfo), typeof(object).GetMethod(nameof(ToString))),
        Expression.Constant(".hydra/context.jsonld"));
      
      var start = model.Uris.Any()
        ? WriteBeginObjectWithContextIdAndType(jsonWriter, contextUri, type, uriResolver)
        : BeginObjectWithContextAndType(jsonWriter, type, contextUri);

      foreach (var exp in start)
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

    static IEnumerable<Expression> WriteBeginObjectWithContextIdAndType(
      ParameterExpression jsonWriter,
      Expression contextUri,
      string type,
      Expression uri)
    {
      foreach (var exp in WriteBeginObjectContext(jsonWriter, contextUri)) yield return exp;
      yield return JsonWriterMethods.WriteRaw(jsonWriter, Nodes.IdProperty);

      yield return JsonWriterMethods.WriteString(jsonWriter, uri);
      yield return JsonWriterMethods.WriteValueSeparator(jsonWriter);

      yield return WritePropertyName(jsonWriter, "@type");
      yield return WriteString(jsonWriter, type);
    }

    static IEnumerable<Expression> WriteBeginObjectContext(ParameterExpression jsonWriter, Expression contextUri)
    {
      yield return JsonWriterMethods.WriteRaw(jsonWriter, Nodes.BeginObjectContext);
      yield return JsonWriterMethods.WriteString(jsonWriter,contextUri);
      yield return JsonWriterMethods.WriteValueSeparator(jsonWriter);
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
      string type, Expression contextUri)
    {
      WriteBeginObjectContext(jsonWriter, contextUri);
      yield return WritePropertyName(jsonWriter, "@type");
      yield return WriteString(jsonWriter, type);
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