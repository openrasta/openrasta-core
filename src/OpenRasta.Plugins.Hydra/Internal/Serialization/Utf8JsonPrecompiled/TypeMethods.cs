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
  public static class StringMethods
  {
    static readonly MethodInfo StringConcatTwoParamsMethodInfo =
      typeof(string).GetMethod(nameof(string.Concat), new[] {typeof(string), typeof(string)});

    public static Expression StringConcat(Expression first, Expression second)
      => Expression.Call(StringConcatTwoParamsMethodInfo, first, second);
  }
  public static class TypeMethods
  {
    static readonly MethodInfo ResolverGetFormatterMethodInfo =
      typeof(CustomResolver).GetMethod(nameof(IJsonFormatterResolver.GetFormatter));

    static readonly PropertyInfo SerializationContextUriResolverPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.UriGenerator));

    static readonly PropertyInfo SerializationContextBaseUriPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.BaseUri));

    public static void ResourceDocument(ParameterExpression jsonWriter,
      ResourceModel model,
      Expression resource,
      Expression options,
      Action<ParameterExpression> defineVar,
      Action<Expression> addStatement, IMetaModelRepository models)
    {
      var uriResolverFunc = Expression.MakeMemberAccess(options, SerializationContextUriResolverPropertyInfo);

      var contextUri = StringMethods.StringConcat(
        Expression.Call(Expression.MakeMemberAccess(options, SerializationContextBaseUriPropertyInfo),
          typeof(object).GetMethod(nameof(ToString))),
        Expression.Constant(".hydra/context.jsonld"));

      var resolver = Expression.Variable(typeof(CustomResolver), "resolver");

      defineVar(resolver);
      addStatement(Expression.Assign(resolver, Expression.New(typeof(CustomResolver))));

      foreach (var exp in WriteBeginObjectContext(jsonWriter, contextUri)) addStatement(exp);

      WriteNode(jsonWriter, model, resource, defineVar, addStatement, uriResolverFunc, models,
        new Stack<ResourceModel>(),
        resolver,
        true);

      addStatement(jsonWriter.WriteEndObject());
    }

    static InvocationExpression InvokeGetCurrentUriExpression(Expression resource, MemberExpression uriResolverFunc)
    {
      return Expression.Invoke(uriResolverFunc, resource);
    }

    static void WriteNode(
      ParameterExpression jsonWriter,
      ResourceModel model,
      Expression resource,
      Action<ParameterExpression> defineVar,
      Action<Expression> addStatement,
      MemberExpression uriResolver,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      ParameterExpression jsonResolver,
      bool hasKeysInObject = false)
    {
      var resourceType = model.ResourceType;
      if (recursionDefender.Contains(model))
        throw new InvalidOperationException(
          $"Detected recursion, already processing {resourceType?.Name}: {string.Join("->", recursionDefender.Select(m => m.ResourceType?.Name).Where(n => n != null))}");


      recursionDefender.Push(model);

      void EnsureKeySeparator(Action<Expression> adder)
      {
        if (hasKeysInObject)
          adder(jsonWriter.WriteValueSeparator());
        hasKeysInObject = true;
      }

      var reosurceRegistrationHydraType = GetHydraTypeName(model);
      var invokeGetCurrentUri = InvokeGetCurrentUriExpression(resource, uriResolver);

      var collectionItemTypes = CollectionItemTypes(resourceType).ToList();

      Type collectionItemType;
      if (collectionItemTypes.Count == 1 &&
          models.TryGetResourceModel((collectionItemType = collectionItemTypes.First()), out _))
      {
        var collectionType =
          typeof(OpenRasta.Plugins.Hydra.Schemas.Hydra.Hydra.Collection<>).MakeGenericType(collectionItemType);
        var collection = Expression.Variable(collectionType);

        defineVar(collection);
        addStatement(Expression.Assign(collection,
          Expression.New(
            collectionType.GetConstructor(new[] {typeof(IEnumerable<>).MakeGenericType(collectionItemType)}),
            resource)));

        resource = collection;
        reosurceRegistrationHydraType = "hydra:Collection"; // hack, lazy, 2am.
        resourceType = collectionType;
      }

      var publicProperties = resourceType
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(IsNotIgnored)
        .ToList();

      var propNames = publicProperties.Select(GetJsonPropertyName);
      var overridesId = propNames.Any(name => name == "@id");
      var overridesType = propNames.Any(name => name == "@type");

      if (overridesId == false && model.Uris.Any())
      {
        EnsureKeySeparator(addStatement);

        foreach (var x in WriteId(jsonWriter, invokeGetCurrentUri)) addStatement(x);
      }
      
      if (overridesType == false)
      {
        EnsureKeySeparator(addStatement);

        foreach (var x in WriteType(jsonWriter, reosurceRegistrationHydraType))
          addStatement(x);
      }

      foreach (var link in model.Links)
      {
        EnsureKeySeparator(addStatement);

        WriteNodeLink(jsonWriter, defineVar, addStatement, link.Relationship, link.Uri, invokeGetCurrentUri, link);
      }

      foreach (var pi in publicProperties)
      {
        if (pi.GetIndexParameters().Any()) continue;

        if (pi.PropertyType.IsValueType && Nullable.GetUnderlyingType(pi.PropertyType) == null)
        {
          
          EnsureKeySeparator(addStatement);

          var propertyGet = Expression.MakeMemberAccess(resource, pi);
          WriteNodePropertyValue(
            jsonWriter,
            defineVar,
            addStatement,
            pi,
            jsonResolver,
            propertyGet);
          continue;
        }
        
        WriteNodeProperty(jsonWriter, resource, defineVar, addStatement, uriResolver, models, recursionDefender, pi,
          jsonResolver, EnsureKeySeparator);
      }

      recursionDefender.Pop();
    }

    static bool IsNotIgnored(PropertyInfo pi)
    {
      return pi.CustomAttributes.Any(a => a.AttributeType.Name == "JsonIgnoreAttribute") == false;
    }


    static string UriStandardCombine(string current, Uri rel) => new Uri(new Uri(current), rel).ToString();

    static string UriSubResourceCombine(string current, Uri rel)
    {
      current = current[current.Length - 1] == '/' ? current : current + "/";
      return new Uri(new Uri(current), rel).ToString();
    }

    static void WriteNodeLink(ParameterExpression jsonWriter, Action<ParameterExpression> defineVar,
      Action<Expression> addStatement, string linkRelationship, Uri linkUri, InvocationExpression uriResolverFunc,
      ResourceLinkModel link)
    {
      addStatement(jsonWriter.WritePropertyName(linkRelationship));
      addStatement(jsonWriter.WriteBeginObject());
      addStatement(jsonWriter.WriteRaw(Nodes.IdProperty));

      var methodInfo = link.CombinationType == ResourceLinkCombination.SubResource
        ? typeof(TypeMethods).GetMethod(nameof(UriSubResourceCombine), BindingFlags.Static | BindingFlags.NonPublic)
        : typeof(TypeMethods).GetMethod(nameof(UriStandardCombine), BindingFlags.Static | BindingFlags.NonPublic);

      var uriCombine = Expression.Call(methodInfo, uriResolverFunc,
        Expression.Constant(linkUri, typeof(Uri)));
      addStatement(jsonWriter.WriteString(uriCombine));


      if (link.Type != null)
      {
        addStatement(jsonWriter.WriteValueSeparator());
        addStatement(jsonWriter.WritePropertyName("@type"));
        addStatement(jsonWriter.WriteString(link.Type));
      }

      addStatement(jsonWriter.WriteEndObject());
    }

    static void WriteNodeProperty(
      ParameterExpression jsonWriter,
      Expression resource,
      Action<ParameterExpression> declareVar,
      Action<Expression> addStatement,
      MemberExpression uriResolverFunc,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      PropertyInfo pi,
      ParameterExpression resolver,
      Action<Action<Expression>> separator
    )
    {
      var propertyStatements = new List<Expression>();
      var propertyVars = new List<ParameterExpression>();

      separator(propertyStatements.Add);

      // var propertyValue;
      var propertyValue = Expression.Variable(pi.PropertyType, $"val{pi.DeclaringType.Name}{pi.Name}");
      declareVar(propertyValue);

      // propertyValue = resource.Property;
      addStatement(Expression.Assign(propertyValue, Expression.MakeMemberAccess(resource, pi)));


      if (models.TryGetResourceModel(pi.PropertyType, out var propertyResourceModel))
      {
        propertyStatements.Add(WritePropertyName(jsonWriter, GetJsonPropertyName(pi)));
        propertyStatements.Add(jsonWriter.WriteBeginObject());
        WriteNode(jsonWriter, propertyResourceModel, propertyValue, propertyVars.Add, propertyStatements.Add,
          uriResolverFunc, models, recursionDefender, resolver);
        propertyStatements.Add(jsonWriter.WriteEndObject());
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

          propertyStatements.Add(WritePropertyName(jsonWriter, GetJsonPropertyName(pi)));

          propertyStatements.Add(jsonWriter.WriteBeginArray());

          itemStatements.Add(Expression.IfThen(Expression.GreaterThan(i, Expression.Constant(0)),
            jsonWriter.WriteValueSeparator()));
          itemStatements.Add(jsonWriter.WriteBeginObject());

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
          itemStatements.Add(jsonWriter.WriteEndObject());
          var loop = Expression.Loop(
            Expression.IfThenElse(
              Expression.LessThan(i, Expression.MakeMemberAccess(itemArray, itemArrayType.GetProperty("Length"))),
              Expression.Block(itemVars.ToArray(), itemStatements.ToArray()),
              Expression.Break(@break)),
            @break
          );
          propertyStatements.Add(loop);
          propertyStatements.Add(jsonWriter.WriteEndArray());
        }
      }

      addStatement(Expression.IfThen(
        Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType)),
        Expression.Block(propertyVars.ToArray(), propertyStatements.ToArray())));
    }

    static IEnumerable<Type> CollectionItemTypes(Type type)
    {
      return from i in type.GetInterfaces()
        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        select i.GetGenericArguments()[0];
    }

    static string GetHydraTypeName(ResourceModel model)
    {
      var hydraResourceModel = model.Hydra();
      return (hydraResourceModel.Vocabulary?.DefaultPrefix == null
               ? string.Empty
               : $"{hydraResourceModel.Vocabulary.DefaultPrefix}:") +
             model.ResourceType.Name;
    }

    static IEnumerable<Expression> WriteId(
      ParameterExpression jsonWriter,
      Expression uri)
    {
      yield return jsonWriter.WriteRaw(Nodes.IdProperty);

      yield return jsonWriter.WriteString(uri);
    }

    public static IEnumerable<Expression> WriteType(ParameterExpression jsonWriter, string type)
    {
      yield return WritePropertyName(jsonWriter, "@type");
      yield return WriteString(jsonWriter, type);
    }

    static IEnumerable<Expression> WriteBeginObjectContext(ParameterExpression jsonWriter, Expression contextUri)
    {
      yield return jsonWriter.WriteRaw(Nodes.BeginObjectContext);
      yield return jsonWriter.WriteString(contextUri);
    }

    static void WriteNodePropertyValue(
      ParameterExpression jsonWriter,
      Action<ParameterExpression> declareVar,
      Action<Expression> addStatement,
      PropertyInfo pi,
      ParameterExpression jsonFormatterResolver, MemberExpression propertyGet)
    {
      var propertyName = GetJsonPropertyName(pi);

      addStatement(WritePropertyName(jsonWriter, propertyName));


      var propertyType = pi.PropertyType;
      var (formatterInstance, serializeMethod) =
        GetFormatter(declareVar, addStatement, jsonFormatterResolver, propertyType);

      var serializeFormatter =
        Expression.Call(formatterInstance, serializeMethod, jsonWriter, propertyGet, jsonFormatterResolver);
      addStatement(serializeFormatter);
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
      return jsonWriter.WriteRaw(bytes);
    }

    public static Expression WriteString(ParameterExpression jsonWriter, string value)
    {
      var writer = new JsonWriter();
      writer.WriteString(value);
      var bytes = writer.ToUtf8ByteArray();
      return jsonWriter.WriteRaw(bytes);
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