using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem.ReflectionBased;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
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

      var contextUri = StringConcat(
        Expression.Call(Expression.MakeMemberAccess(options, SerializationContextBaseUriPropertyInfo),
          typeof(object).GetMethod(nameof(ToString))),
        Expression.Constant(".hydra/context.jsonld"));

      var resolver = Expression.Variable(typeof(CustomResolver), "resolver");
      variable(resolver);
      statement(Expression.Assign(resolver, Expression.New(typeof(CustomResolver))));

      foreach (var exp in WriteBeginObjectContext(jsonWriter, contextUri)) statement(exp);

      WriteNode(jsonWriter, model, resource, variable, statement, uriResolverFunc, models, new Stack<ResourceModel>(),
        resolver);

      statement(JsonWriterMethods.WriteEndObject(jsonWriter));
    }

    static InvocationExpression GetUri(Expression resource, MemberExpression uriResolverFunc)
    {
      return Expression.Invoke(uriResolverFunc, resource);
    }

    static void WriteNode(
      ParameterExpression jsonWriter,
      ResourceModel model,
      Expression resource,
      Action<ParameterExpression> var,
      Action<Expression> statement,
      MemberExpression uriResolver,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      ParameterExpression jsonResolver)
    {
      var resourceType = model.ResourceType;
      if (recursionDefender.Contains(model))
        throw new InvalidOperationException(
          $"Detected recursion, already processing {resourceType?.Name}: {string.Join("->", recursionDefender.Select(m => m.ResourceType?.Name).Where(n => n != null))}");

      recursionDefender.Push(model);

      var type = GetTypeName(models, model);
      var uri = GetUri(resource, uriResolver);

      var collectionItemTypes = CollectionItemTypes(resourceType).ToList();

      Type collectionItemType;
      if (collectionItemTypes.Count() == 1 &&
          models.TryGetResourceModel((collectionItemType = collectionItemTypes.First()), out _))
      {
        var collectionType =
          typeof(OpenRasta.Plugins.Hydra.Schemas.Hydra.Hydra.Collection<>).MakeGenericType(collectionItemType);
        var collection = Expression.Variable(collectionType);
        var(collection);
        statement(Expression.Assign(collection,
          Expression.New(
            collectionType.GetConstructor(new[] {typeof(IEnumerable<>).MakeGenericType(collectionItemType)}),
            resource)));
        resource = collection;
        type = "hydra:Collection"; // hack, lazy, 2am.
        resourceType = collectionType;
      }

      foreach (var exp in model.Uris.Any()
        ? WriteIdType(jsonWriter, type, uri, model)
        : WriteType(jsonWriter, type))
        statement(exp);


      foreach (var pi in resourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
        if (pi.CustomAttributes.Any(a => a.AttributeType.Name == "JsonIgnoreAttribute"))
          continue;

        if (pi.GetIndexParameters().Any()) continue;

        if (pi.PropertyType.IsValueType && Nullable.GetUnderlyingType(pi.PropertyType) == null)
        {
          WriteNodePropertyValue(jsonWriter, var, statement, pi, jsonResolver,
            Expression.MakeMemberAccess(resource, pi));
          continue;
        }


        WriteNodeProperty(jsonWriter, resource, var, statement, uriResolver, models, recursionDefender, pi,
          jsonResolver);
      }

      recursionDefender.Pop();
    }

    static void WriteNodeProperty(ParameterExpression jsonWriter, Expression resource,
      Action<ParameterExpression> variable, Action<Expression> statement,
      MemberExpression uriResolverFunc, IMetaModelRepository models, Stack<ResourceModel> recursionDefender,
      PropertyInfo pi,
      ParameterExpression resolver)
    {
      var propertyStatements = new List<Expression>();
      var propertyVars = new List<ParameterExpression>();

      // var propertyValue;
      var propertyValue = Expression.Variable(pi.PropertyType, $"val{pi.DeclaringType.Name}{pi.Name}");
      variable(propertyValue);

      // propertyValue = resource.Property;
      statement(Expression.Assign(propertyValue, Expression.MakeMemberAccess(resource, pi)));


      if (models.TryGetResourceModel(pi.PropertyType, out var propertyResourceModel))
      {
        // property has a registration, it's either an iri node or a blank node
        propertyStatements.Add(JsonWriterMethods.WriteValueSeparator(jsonWriter));
        propertyStatements.Add(WritePropertyName(jsonWriter, GetJsonPropertyName(pi)));
        propertyStatements.Add(JsonWriterMethods.WriteBeginObject(jsonWriter));
        WriteNode(jsonWriter, propertyResourceModel, propertyValue, propertyVars.Add, propertyStatements.Add,
          uriResolverFunc, models, recursionDefender, resolver);
        propertyStatements.Add(JsonWriterMethods.WriteEndObject(jsonWriter));
      }
      else
      {
        // not an iri node itself, but is it a list of nodes?
        var itemResourceRegistrations = (
          from i in pi.PropertyType.GetInterfaces()
          where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
          let itemType = i.GetGenericArguments()[0]
          where itemType != typeof(object)
          let resourceModels = models.ResourceRegistrations.Where(r => itemType.IsAssignableFrom(r.ResourceType))
          where resourceModels.Any()
          orderby resourceModels.Count() descending
          select new
          {
            itemType, models =
              (from possible in resourceModels
                orderby possible.ResourceType.GetInheritanceDistance(itemType)
                select possible).ToList()
          }).FirstOrDefault();

        if (itemResourceRegistrations == null)
        {
          // not a list of iri or blank nodes
          WriteNodePropertyValue(jsonWriter, propertyVars.Add, propertyStatements.Add, pi, resolver,
            Expression.MakeMemberAccess(resource, pi));
        }
        else
        {
          // it's a list of nodes
          var itemArrayType = itemResourceRegistrations.itemType.MakeArrayType();
          var itemArray = Expression.Variable(itemArrayType);

          var toArrayMethod = typeof(Enumerable).GetMethod("ToArray")
            .MakeGenericMethod(itemResourceRegistrations.itemType);
          var assign = Expression.Assign(itemArray, Expression.Call(toArrayMethod, propertyValue));
          propertyVars.Add(itemArray);
          propertyStatements.Add(assign);

          var i = Expression.Variable(typeof(int));
          propertyVars.Add(i);

          var initialValue = Expression.Assign(i, Expression.Constant(0));
          propertyStatements.Add(initialValue);

          var itemVars = new List<ParameterExpression>();
          var itemStatements = new List<Expression>();

          var @break = Expression.Label("break");

          propertyStatements.Add(JsonWriterMethods.WriteValueSeparator(jsonWriter));
          propertyStatements.Add(WritePropertyName(jsonWriter, GetJsonPropertyName(pi)));

          propertyStatements.Add(JsonWriterMethods.WriteBeginArray(jsonWriter));

          itemStatements.Add(Expression.IfThen(Expression.GreaterThan(i, Expression.Constant(0)),
            JsonWriterMethods.WriteValueSeparator(jsonWriter)));
          itemStatements.Add(JsonWriterMethods.WriteBeginObject(jsonWriter));

          BlockExpression resourceBlock(ResourceModel r, ParameterExpression typed)
          {
            var vars = new List<ParameterExpression>();
            var statements = new List<Expression>();
            WriteNode(
              jsonWriter,
              r,
              typed,
              vars.Add, statements.Add,
              uriResolverFunc, models, recursionDefender, resolver);
            return Expression.Block(vars.ToArray(), statements.ToArray());
          }


          Expression renderBlock =
            Expression.Block(Expression.Throw(Expression.New(typeof(InvalidOperationException))));

          // with C : B : A, if is C else if is B else if is A else throw

          foreach (var specificModel in itemResourceRegistrations.models)
          {
            var typed = Expression.Variable(specificModel.ResourceType, "as" + specificModel.ResourceType.Name);
            itemVars.Add(typed);
            var @as = Expression.Assign(typed,
              Expression.TypeAs(Expression.ArrayAccess(itemArray, i), specificModel.ResourceType));
            renderBlock = Expression.IfThenElse(
              Expression.NotEqual(@as, Expression.Default(specificModel.ResourceType)),
              resourceBlock(specificModel, @typed),
              renderBlock);
          }

          itemStatements.Add(renderBlock);
          itemStatements.Add(Expression.PostIncrementAssign(i));
          itemStatements.Add(JsonWriterMethods.WriteEndObject(jsonWriter));
          var loop = Expression.Loop(
            Expression.IfThenElse(
              Expression.LessThan(i, Expression.MakeMemberAccess(itemArray, itemArrayType.GetProperty("Length"))),
              Expression.Block(itemVars.ToArray(), itemStatements.ToArray()),
              Expression.Break(@break)),
            @break
          );
          propertyStatements.Add(loop);
          propertyStatements.Add(JsonWriterMethods.WriteEndArray(jsonWriter));
        }
      }

      statement(Expression.IfThen(
        Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType)),
        Expression.Block(propertyVars.ToArray(), propertyStatements.ToArray())));
    }

    static IEnumerable<Type> CollectionItemTypes(Type type)
    {
      return from i in type.GetInterfaces()
        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        select i.GetGenericArguments()[0];
    }

    static string GetTypeName(IMetaModelRepository models, ResourceModel model)
    {
      var opts = models.CustomRegistrations.OfType<HydraOptions>().Single();
      var hydraResourceModel = model.Hydra();
      return (hydraResourceModel.Vocabulary?.DefaultPrefix == null
               ? string.Empty
               : $"{hydraResourceModel.Vocabulary.DefaultPrefix}:") +
             model.ResourceType.Name;
    }

    static IEnumerable<Expression> WriteIdType(ParameterExpression jsonWriter,
      string type,
      Expression uri, ResourceModel model)
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

    static void WriteNodePropertyValue(ParameterExpression jsonWriter,
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

      statement(Expression.IfThen(Expression.Equal(formatterInstance, Expression.Default(jsonFormatterType)),
        Expression.Throw(Expression.New(typeof(ArgumentNullException)))));
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

    public static IEnumerable<Type> GetInheritanceHierarchy(this Type type)
    {
      for (var current = type; current != null; current = current.BaseType)
      {
        yield return current;
      }
    }

    public static string GetRange(this Type type)
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
  }
}