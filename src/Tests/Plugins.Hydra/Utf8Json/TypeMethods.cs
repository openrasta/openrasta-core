using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Internal;
using OpenRasta.TypeSystem.ReflectionBased;
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

    public static void ResourceDocument(ParameterExpression jsonWriter,
      ResourceModel model,
      Expression resource,
      Expression options,
      Action<ParameterExpression> variable,
      Action<Expression> statement, IMetaModelRepository models)
    {
      var uriResolverFunc = Expression.MakeMemberAccess(options, SerializationContextUriResolverPropertyInfo);
      var uriResolver = Expression.Invoke(uriResolverFunc, resource);
      var contextUri = StringConcat(
        Expression.Call(Expression.MakeMemberAccess(options, SerializationContextBaseUriPropertyInfo),
          typeof(object).GetMethod(nameof(ToString))),
        Expression.Constant(".hydra/context.jsonld"));

      foreach (var exp in WriteBeginObjectContext(jsonWriter, contextUri)) statement(exp);

      Resource(jsonWriter, model, resource, variable, statement, uriResolver, models);

      statement(JsonWriterMethods.WriteEndObject(jsonWriter));
    }

    static void Resource(ParameterExpression jsonWriter, ResourceModel model, Expression resource,
      Action<ParameterExpression> variable,
      Action<Expression> statement,
      InvocationExpression uriResolver,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender = null)
    {
      if (recursionDefender == null) recursionDefender = new Stack<ResourceModel>();
      else if (recursionDefender.Contains(model))
        throw new InvalidOperationException("Detected recursion in model handling");

      recursionDefender.Push(model);
      var type = model.ResourceType.Name;
      foreach (var exp in model.Uris.Any()
        ? WriteIdType(jsonWriter, type, uriResolver)
        : WriteType(jsonWriter, type))
        statement(exp);

      var resolver = Expression.Variable(typeof(CustomResolver), "resolver");
      variable(resolver);
      statement(Expression.Assign(resolver, Expression.New(typeof(CustomResolver))));

      foreach (var pi in model.ResourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
        if (pi.CustomAttributes.Any(a => a.AttributeType.Name == "JsonIgnoreAttribute"))
          continue;


        if (pi.PropertyType.IsValueType)
        {
          WriteResourceProperty(jsonWriter, variable, statement, pi, resolver,
            Expression.MakeMemberAccess(resource, pi));
          continue;
        }


        var propertyStatements = new List<Expression>();
        var propertyVars = new List<ParameterExpression>();

        // var propertyValue;
        var propertyValue = Expression.Variable(pi.PropertyType);
        variable(propertyValue);

        // propertyValue = resource.Property;
        statement(Expression.Assign(propertyValue, Expression.MakeMemberAccess(resource, pi)));

        if (models.TryGetResourceModel(pi.PropertyType, out var propertyResourceModel))
        {
          propertyStatements.Add(JsonWriterMethods.WriteValueSeparator(jsonWriter));
          propertyStatements.Add(WritePropertyName(jsonWriter, GetJsonPropertyName(pi)));
          propertyStatements.Add(JsonWriterMethods.WriteBeginObject(jsonWriter));
          Resource(jsonWriter, propertyResourceModel, propertyValue, propertyVars.Add, propertyStatements.Add,
            uriResolver, models, recursionDefender);
          propertyStatements.Add(JsonWriterMethods.WriteEndObject(jsonWriter));
        }
        else
        {
          WriteResourceProperty(jsonWriter, propertyVars.Add, propertyStatements.Add, pi, resolver,
            Expression.MakeMemberAccess(resource, pi));
        }

        statement(Expression.IfThen(
          Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType)),
          Expression.Block(propertyVars.ToArray(), propertyStatements.ToArray())));
      }

      recursionDefender.Pop();
    }

    static IEnumerable<Expression> WriteIdType(
      ParameterExpression jsonWriter,
      string type,
      Expression uri)
    {
      yield return JsonWriterMethods.WriteRaw(jsonWriter, Nodes.IdProperty);

      yield return JsonWriterMethods.WriteString(jsonWriter, uri);
      yield return JsonWriterMethods.WriteValueSeparator(jsonWriter);

      yield return WritePropertyName(jsonWriter, "@type");
      yield return WriteString(jsonWriter, type);
    }

    public static IEnumerable<Expression> WriteType(ParameterExpression jsonWriter, string type)
    {
      yield return WritePropertyName(jsonWriter, "@type");
      yield return WriteString(jsonWriter, type);
    }

    static IEnumerable<Expression> WriteBeginObjectContext(ParameterExpression jsonWriter, Expression contextUri)
    {
      yield return JsonWriterMethods.WriteRaw(jsonWriter, Nodes.BeginObjectContext);
      yield return JsonWriterMethods.WriteString(jsonWriter, contextUri);
      yield return JsonWriterMethods.WriteValueSeparator(jsonWriter);
    }

    static void WriteResourceProperty(ParameterExpression jsonWriter,
      Action<ParameterExpression> variable,
      Action<Expression> statement,
      PropertyInfo pi,
      ParameterExpression jsonFormatterResolver, MemberExpression propertyGet)
    {
      statement(JsonWriterMethods.WriteValueSeparator(jsonWriter));
      var propertyName = GetJsonPropertyName(pi);


      statement(WritePropertyName(jsonWriter, propertyName));


      var propertyType = pi.PropertyType;
      var (formatterInstance, serializeMethod) = GetFormatter(variable, statement, jsonFormatterResolver, propertyType);

      var serializeFormatter =
        Expression.Call(formatterInstance, serializeMethod, jsonWriter, propertyGet, jsonFormatterResolver);
      statement(serializeFormatter);
    }

    static string GetJsonPropertyName(PropertyInfo pi)
    {
      return pi.CustomAttributes
               .Where(a => a.AttributeType.Name == "JsonPropertyAttribute")
               .SelectMany(a => a.ConstructorArguments)
               .Where(a => a.ArgumentType == typeof(string))
               .Select(a => (string) a.Value)
               .FirstOrDefault() ?? ToCamelCase(pi.Name);
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