using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;
using OpenRasta.TypeSystem.ReflectionBased;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class TypeMethods
  {
    static readonly MethodInfo ResolverGetFormatterMethodInfo =
      typeof(CustomResolver).GetMethod(nameof(IJsonFormatterResolver.GetFormatter));

    static readonly MethodInfo EnumerableToArrayMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));

    public static CodeBlock ResourceDocument(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      Variable<SerializationContext> options,
      IMetaModelRepository models)
    {
      return CodeBlock.Convert((param, statement) => ResourceDocument(
        jsonWriter,
        model,
        resource,
        options,
        param, statement,
        models));
    }

    static void ResourceDocument(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      Variable<SerializationContext> options,
      Action<ParameterExpression> defineVar,
      Action<Expression> addStatement,
      IMetaModelRepository models)
    {
      var uriResolverFunc = options.get_UriGenerator();

      var converter = options.get_BaseUri().ObjectToString();
      var contextUri = StringMethods.Concat(converter, New.Const(".hydra/context.jsonld"));

      var resolver = New.Var<CustomResolver>("resolver");

      defineVar(resolver);
      addStatement(Expression.Assign(resolver, Expression.New(typeof(CustomResolver))));

      foreach (var exp in WriteBeginObjectContext(jsonWriter, contextUri)) addStatement(exp);

      WriteNode(jsonWriter, model, resource, defineVar, addStatement, uriResolverFunc, models,
        new Stack<ResourceModel>(),
        resolver, true);

      addStatement(jsonWriter.WriteEndObject());
    }

    static InvocationExpression InvokeGetCurrentUriExpression(Expression resource, MemberExpression uriResolverFunc)
    {
      return Expression.Invoke(uriResolverFunc, resource);
    }

    static CodeBlock WriteNode(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      MemberExpression uriResolver,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      ParameterExpression jsonResolver)
    {
      return CodeBlock.Convert((param, statement) => WriteNode(jsonWriter, model, resource, param, statement,
        uriResolver, models, recursionDefender, jsonResolver));
    }

    static void WriteNode(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      Action<ParameterExpression> defineVar,
      Action<Expression> addStatement,
      MemberExpression uriResolver,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      ParameterExpression jsonResolver,
      bool hasExistingNodeProperty  = false)
    {
      var resourceType = model.ResourceType;

      if (recursionDefender.Contains(model))
        throw new InvalidOperationException(
          $"Detected recursion, already processing {resourceType?.Name}: {string.Join("->", recursionDefender.Select(m => m.ResourceType?.Name).Where(n => n != null))}");


      recursionDefender.Push(model);

      var resourceRegistrationHydraType = HydraTextExtensions.GetHydraTypeName(model);
      var invokeGetCurrentUri = InvokeGetCurrentUriExpression(resource, uriResolver);

      var collectionItemTypes = HydraTextExtensions.CollectionItemTypes(resourceType).ToList();

      Type collectionItemType;
      if (collectionItemTypes.Count == 1 &&
          models.TryGetResourceModel(collectionItemType = collectionItemTypes.First(), out _))
      {
        var collectionType = HydraTypes.Collection.MakeGenericType(collectionItemType);
        var collectionCtor =
          collectionType.GetConstructor(new[] {typeof(IEnumerable<>).MakeGenericType(collectionItemType)});
        var collection = Expression.Variable(collectionType);

        defineVar(collection);
        addStatement(Expression.Assign(collection,
          Expression.New(
            collectionCtor,
            resource)));

        resource = collection;

        // if we have a generic list of sort, we hydra:Collection instead
        if (resourceType.IsGenericType) // IEnum<T>, List<T> etc
          resourceRegistrationHydraType = "hydra:Collection";

        resourceType = collectionType;
      }

      var properties = GetNodeProperties(jsonWriter, model, resource, uriResolver, models, recursionDefender, jsonResolver, resourceType, invokeGetCurrentUri, resourceRegistrationHydraType);

      if (properties.Any() == false) return;
      
      var alwaysWrittenProperties = properties.Where(p => p.Conditional == null).ToList();
      var conditionalProperties = properties.Where(p => p.Conditional != null).ToList();


      void renderProperty(NodeProperty prop, InlineCode separatorCode)
      {
        prop.Preamble?.CopyTo(defineVar, addStatement);

        var code = prop.Code;
        if (separatorCode != null)
          code = new InlineCode(separatorCode, code);

        if (prop.Conditional != null)
          code = new InlineCode(
            Expression.IfThen(
              prop.Conditional,
              new CodeBlock(code)));
        
        code.CopyTo(defineVar, addStatement);
      }

      InlineCode separator;
      if (hasExistingNodeProperty || alwaysWrittenProperties.Any())
      {
        separator = new InlineCode(jsonWriter.WriteValueSeparator());
      }
      else
      {
        var hasProp = New.Var<bool>("hasProp");
        
        defineVar(hasProp);
        addStatement(hasProp.Assign(false));

        separator = new InlineCode(
          If.Then(
            hasProp.EqualTo(false),
            Expression.Block(jsonWriter.WriteValueSeparator(),
              hasProp.Assign(true))
          ));
      }

      var sortedProperties = alwaysWrittenProperties.Concat(conditionalProperties).ToArray();
      
      renderProperty(sortedProperties[0], hasExistingNodeProperty  ? separator : null);
      
      foreach (var property in sortedProperties.Skip(1))
      {
        renderProperty(property, separator);
      }

      recursionDefender.Pop();
    }

    static IEnumerable<NodeProperty> GetNodeProperties(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      MemberExpression uriResolver,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      ParameterExpression jsonResolver,
      Type resourceType,
      InvocationExpression invokeGetCurrentUri,
      string resourceRegistrationHydraType)
    {
      var publicProperties = resourceType
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(HydraTextExtensions.IsNotIgnored)
        .ToList();

      var propNames = publicProperties.Select(HydraTextExtensions.GetJsonPropertyName);
      var overridesId = propNames.Any(name => name == "@id");
      var overridesType = propNames.Any(name => name == "@type");
      
      if (overridesId == false && model.Uris.Any())
      {
        yield return WriteId(jsonWriter, invokeGetCurrentUri);
      }

      if (overridesType == false)
      {
        yield return WriteType(jsonWriter, resourceRegistrationHydraType);
      }


      foreach (var pi in publicProperties)
      {
        if (pi.GetIndexParameters().Any()) continue;

        if (pi.PropertyType.IsValueType && Nullable.GetUnderlyingType(pi.PropertyType) == null)
        {
          var nodePropertyValue = WriteNodePropertyValue(
            jsonWriter,
            pi,
            jsonResolver,
            resource);

          yield return nodePropertyValue;
          continue;
        }

        yield return WriteNodeProperty(
          jsonWriter, resource, uriResolver, models, recursionDefender, pi,
          jsonResolver);
        ;
      }

      foreach (var link in model.Links)
      {
        yield return WriteNodeLink(jsonWriter, link.Relationship, link.Uri, invokeGetCurrentUri, link);
      }
    }


    static NodeProperty WriteNodeLink(
      Variable<JsonWriter> jsonWriter,
      string linkRelationship,
      Uri linkUri,
      InvocationExpression uriResolverFunc,
      ResourceLinkModel link)
    {
      IEnumerable<AnyExpression> getNodeLink()
      {
        yield return jsonWriter.WritePropertyName(linkRelationship);
        yield return jsonWriter.WriteBeginObject();
        yield return jsonWriter.WriteRaw(Nodes.IdProperty);

        var methodInfo = link.CombinationType == ResourceLinkCombination.SubResource
          ? typeof(HydraTextExtensions).GetMethod(nameof(HydraTextExtensions.UriSubResourceCombine),
            BindingFlags.Static | BindingFlags.NonPublic)
          : typeof(HydraTextExtensions).GetMethod(nameof(HydraTextExtensions.UriStandardCombine),
            BindingFlags.Static | BindingFlags.NonPublic);

        var uriCombine = Expression.Call(methodInfo, uriResolverFunc,
          Expression.Constant(linkUri, typeof(Uri)));
        yield return jsonWriter.WriteString(uriCombine);

        if (link.Type != null)
        {
          yield return jsonWriter.WriteValueSeparator();
          yield return jsonWriter.WritePropertyName("@type");
          yield return jsonWriter.WriteString(link.Type);
        }

        yield return jsonWriter.WriteEndObject();
      }

      return new NodeProperty(linkRelationship) {Code = new InlineCode(getNodeLink())};
    }

    static NodeProperty WriteNodeProperty(
      Variable<JsonWriter> jsonWriter,
      Expression resource,
      MemberExpression uriResolverFunc,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      PropertyInfo pi,
      ParameterExpression resolver
    )
    {
      // var propertyValue;
      var propertyValue = Expression.Variable(pi.PropertyType, $"val{pi.DeclaringType.Name}{pi.Name}");

      // propertyValue = resource.Property;
      var propertyValueAssignment = Expression.Assign(propertyValue, Expression.MakeMemberAccess(resource, pi));

      var preamble = new InlineCode(new Expression[] {propertyValue, propertyValueAssignment});

      if (models.TryGetResourceModel(pi.PropertyType, out var propertyResourceModel))
      {
        var jsonPropertyName = HydraTextExtensions.GetJsonPropertyName(pi);
        return new NodeProperty(jsonPropertyName)
        {
          Preamble = preamble,
          Code = new InlineCode(new[]
          {
            jsonWriter.WritePropertyName(jsonPropertyName),
            jsonWriter.WriteBeginObject(),
            WriteNode(jsonWriter, propertyResourceModel, propertyValue,
              uriResolverFunc, models, recursionDefender, resolver),
            jsonWriter.WriteEndObject()
          }),
          Conditional = Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType))
        };
      }

      // not an iri node itself, but is it a list of nodes?
      var itemResourceRegistrations = (
        from i in pi.PropertyType.GetInterfaces()
        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        let itemType = i.GetGenericArguments()[0]
        where itemType != typeof(object)
        let resourceModels = models.ResourceRegistrations.Where(r => itemType.IsAssignableFrom(r.ResourceType))
        where resourceModels.Any()
        orderby resourceModels.Count() descending
        select
        (
          itemType,
          (from possible in resourceModels
            orderby possible.ResourceType.GetInheritanceDistance(itemType)
            select possible).ToList()
        )).ToList<(Type itemType, List<ResourceModel> models)>();

      if (itemResourceRegistrations.Any() == false)
      {
        // not a list of iri or blank nodes
        var propValue = WriteNodePropertyValue(jsonWriter, pi, resolver, resource);
        propValue.Preamble = preamble;
        propValue.Conditional = Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType));
        return propValue;
      }

      // it's a list of nodes

      return WriteNodeList(jsonWriter, uriResolverFunc, models, recursionDefender, pi, resolver,
        itemResourceRegistrations, propertyValue, preamble);
    }

    static NodeProperty WriteNodeList(
      Variable<JsonWriter> jsonWriter,
      MemberExpression uriResolverFunc,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      PropertyInfo pi,
      ParameterExpression resolver,
      List<(Type itemType, List<ResourceModel> models)> itemResourceRegistrations,
      ParameterExpression propertyValue,
      InlineCode preamble)
    {
      var propertyStatements = new List<Expression>();
      var propertyVars = new List<ParameterExpression>();
      var itemVars = new List<ParameterExpression>();
      var itemStatements = new List<Expression>();

      var itemRegistration = itemResourceRegistrations.First();

      var itemArrayType = itemRegistration.itemType.MakeArrayType();
      var itemArray = Expression.Variable(itemArrayType);

      var toArrayMethod = EnumerableToArrayMethodInfo
        .MakeGenericMethod(itemRegistration.itemType);
      var assign = Expression.Assign(itemArray, Expression.Call(toArrayMethod, propertyValue));
      propertyVars.Add(itemArray);
      propertyStatements.Add(assign);

      var arrayIndex = New.Var<int>();

      propertyVars.Add(arrayIndex);
      propertyStatements.Add(arrayIndex.Assign(0));

      var @break = Expression.Label("break");

      var jsonPropertyName = HydraTextExtensions.GetJsonPropertyName(pi);
      propertyStatements.Add(jsonWriter.WritePropertyName(jsonPropertyName));

      propertyStatements.Add(jsonWriter.WriteBeginArray());

      itemStatements.Add(If.Then(
        arrayIndex.GreaterThan(0),
        jsonWriter.WriteValueSeparator()));

      itemStatements.Add(jsonWriter.WriteBeginObject());

      CodeBlock resourceBlock(ResourceModel r, ParameterExpression typed)
      {
        return WriteNode(
          jsonWriter,
          r,
          typed,
          uriResolverFunc,
          models,
          recursionDefender,
          resolver);
      }

      Expression renderBlock =
        Expression.Block(Expression.Throw(Expression.New(typeof(InvalidOperationException))));

      // with C : B : A, if is C else if is B else if is A else throw

      foreach (var specificModel in itemRegistration.models)
      {
        var typed = Expression.Variable(specificModel.ResourceType, "as" + specificModel.ResourceType.Name);
        itemVars.Add(typed);
        var @as = Expression.Assign(typed,
          Expression.TypeAs(Expression.ArrayAccess(itemArray, arrayIndex), specificModel.ResourceType));
        renderBlock = Expression.IfThenElse(
          Expression.NotEqual(@as, Expression.Default(specificModel.ResourceType)),
          resourceBlock(specificModel, @typed),
          renderBlock);
      }

      itemStatements.Add(renderBlock);
      itemStatements.Add(Expression.PostIncrementAssign(arrayIndex));
      itemStatements.Add(jsonWriter.WriteEndObject());
      var loop = Expression.Loop(
        If.ThenElse(
          arrayIndex.LessThan(Expression.MakeMemberAccess(itemArray, itemArrayType.GetProperty("Length"))),
          Expression.Block(itemVars.ToArray(), itemStatements.ToArray()),
          Expression.Break(@break)),
        @break
      );
      propertyStatements.Add(loop);
      propertyStatements.Add(jsonWriter.WriteEndArray());

      return new NodeProperty(jsonPropertyName)
      {
        Preamble = preamble,
        Code = new InlineCode(new[]
        {
          Expression.Block(propertyVars.ToArray(), propertyStatements.ToArray())
        }),
        Conditional = Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType))
      };
    }

    static NodeProperty WriteId(
      Variable<JsonWriter> jsonWriter,
      Expression uri)
    {
      return new NodeProperty("@id")
      {
        Code = new InlineCode(jsonWriter.WriteRaw(Nodes.IdProperty), jsonWriter.WriteString(uri))
      };
    }

    public static NodeProperty WriteType(Variable<JsonWriter> jsonWriter, string type)
    {
      return new NodeProperty("@type")
      {
        Code =
          new InlineCode(jsonWriter.WriteRaw(Nodes.TypeProperty), jsonWriter.WriteString(type))
      };
    }

    static IEnumerable<Expression> WriteBeginObjectContext(Variable<JsonWriter> jsonWriter, Expression contextUri)
    {
      yield return jsonWriter.WriteRaw(Nodes.BeginObjectContext);
      yield return jsonWriter.WriteString(contextUri);
    }

    static NodeProperty WriteNodePropertyValue(
      Variable<JsonWriter> jsonWriter,
      PropertyInfo pi,
      ParameterExpression jsonFormatterResolver,
      Expression resource)
    {
      var propertyGet = Expression.MakeMemberAccess(resource, pi);
      var propertyName = HydraTextExtensions.GetJsonPropertyName(pi);
      var propertyType = pi.PropertyType;

      return new NodeProperty(propertyName)
      {
        Code = new InlineCode(
          jsonWriter.WritePropertyName(propertyName),
          GetFormatter(jsonFormatterResolver, propertyType, jsonWriter, propertyGet))
      };
    }


    static InlineCode GetFormatter(
      ParameterExpression jsonFormatterResolver,
      Type propertyType,
      Variable<JsonWriter> jsonWriter,
      MemberExpression propertyGet)
    {
      IEnumerable<AnyExpression> getFormatter()
      {
        var resolverGetFormatter = ResolverGetFormatterMethodInfo.MakeGenericMethod(propertyType);
        var jsonFormatterType = typeof(IJsonFormatter<>).MakeGenericType(propertyType);
        var serializeMethod = jsonFormatterType.GetMethod("Serialize",
          new[] {typeof(JsonWriter).MakeByRefType(), propertyType, typeof(IJsonFormatterResolver)});

        var formatterInstance = Expression.Variable(jsonFormatterType);
        return new AnyExpression[]
        {
          formatterInstance,

          Expression.Assign(formatterInstance, Expression.Call(jsonFormatterResolver, resolverGetFormatter)),

          Expression.IfThen(
            Expression.Equal(formatterInstance,
              Expression.Default(jsonFormatterType)),
            Expression.Throw(Expression.New(typeof(ArgumentNullException)))),

          Expression.Call(formatterInstance, serializeMethod, jsonWriter, propertyGet, jsonFormatterResolver)
        };
      }

      return new InlineCode(getFormatter());
    }
  }
}