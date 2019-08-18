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
      typeof(HydraJsonFormatterResolver).GetMethod(nameof(IJsonFormatterResolver.GetFormatter));

    static readonly MethodInfo EnumerableToArrayMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));


    public static CodeBlock ResourceDocument(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      Variable<SerializationContext> options,
      IMetaModelRepository models)
    {
      var uriGenerator = options.get_UriGenerator();
      var baseUri = options.get_BaseUri().ObjectToString();
      var contextUri = StringMethods.Concat(baseUri, New.Const(".hydra/context.jsonld"));

      var jsonFormatterResolver = New.Var<HydraJsonFormatterResolver>("resolver");
      var assignResolver = jsonFormatterResolver.Assign(New.Instance<HydraJsonFormatterResolver>());

      var rootNode = WriteNode(
        jsonWriter,
        model,
        resource,
        uriGenerator,
        models,
        jsonFormatterResolver,
        new[]
        {
          WriteContext(jsonWriter,contextUri)
        });
      
      return new CodeBlock(
        jsonFormatterResolver,
        assignResolver,
        jsonWriter.WriteBeginObject(),
        rootNode,
        jsonWriter.WriteEndObject()
      );
    }

    static CodeBlock WriteNode(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      MemberAccess<Func<object, string>> uriGenerator,
      IMetaModelRepository models,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
      NodeProperty[] existingNodeProperties =  null,
      Stack<ResourceModel> recursionDefender = null)
    {
      recursionDefender = recursionDefender ?? new Stack<ResourceModel>();
      var resourceType = model.ResourceType;

      if (recursionDefender.Contains(model))
        throw new InvalidOperationException(
          $"Detected recursion, already processing {resourceType?.Name}: {string.Join("->", recursionDefender.Select(m => m.ResourceType?.Name).Where(n => n != null))}");

      recursionDefender.Push(model);

      var resourceRegistrationHydraType = HydraTextExtensions.GetHydraTypeName(model);
      var resourceUri = uriGenerator.Invoke(resource);

      List<NodeProperty> nodeProperties  = new List<NodeProperty>(existingNodeProperties ?? Enumerable.Empty<NodeProperty>());
      
      nodeProperties.AddRange(GetNodeProperties(jsonWriter, model, resource, uriGenerator, models, recursionDefender,
        jsonFormatterResolver, resourceType, resourceUri, resourceRegistrationHydraType));


      var collectionItemTypes = HydraTextExtensions.CollectionItemTypes(resourceType).ToList();
      Type collectionItemType = null;
      var isHydraCollection = collectionItemTypes.Count == 1 &&
                              models.TryGetResourceModel(collectionItemType = collectionItemTypes.First(), out _);

      IEnumerable<AnyExpression> render()
      {
        if (isHydraCollection)
        {
          var collectionType = HydraTypes.Collection.MakeGenericType(collectionItemType);
          var collectionCtor =
            collectionType.GetConstructor(new[] {typeof(IEnumerable<>).MakeGenericType(collectionItemType)});
          var collection = Expression.Variable(collectionType);

          yield return collection;
          var instantiateCollection = Expression.Assign(collection, Expression.New(collectionCtor, resource));
          yield return (instantiateCollection);

          resource = collection;

          // if we have a generic list of sort, we hydra:Collection instead
          if (resourceType.IsGenericType) // IEnum<T>, List<T> etc
            resourceRegistrationHydraType = "hydra:Collection";

          resourceType = collectionType;
          nodeProperties.AddRange(GetNodeProperties(jsonWriter, model, resource, uriGenerator, models, recursionDefender,
            jsonFormatterResolver, resourceType, resourceUri, resourceRegistrationHydraType));
        }

        if (nodeProperties.Any())
          yield return WriteNodeProperties(jsonWriter, nodeProperties);
      }

      recursionDefender.Pop();
      return new CodeBlock(render());
    }

    static InlineCode WriteNodeProperties(
      Variable<JsonWriter> jsonWriter,
      List<NodeProperty> nodeProperties)
    {
      nodeProperties = nodeProperties.OrderBy(p => p.Name).ToList();
      var alwaysWrittenProperties = nodeProperties.Where(p => p.Conditional == null).ToList();
      var conditionalProperties = nodeProperties.Where(p => p.Conditional != null).ToList();


      InlineCode convertPropertyToCode(NodeProperty prop, InlineCode separatorCode)
      {
        var code = prop.Code;
        if (separatorCode != null)
          code = new InlineCode(separatorCode, code);

        if (prop.Conditional != null)
          code = new InlineCode(
            Expression.IfThen(
              prop.Conditional,
              new CodeBlock(code)));

        return new InlineCode(prop.Preamble, code);
      }

      IEnumerable<AnyExpression> renderNode()
      {
        InlineCode separator;
        if (alwaysWrittenProperties.Any())
        {
          separator = new InlineCode(jsonWriter.WriteValueSeparator());
        }
        else
        {
          var hasProp = New.Var<bool>("hasProp");

          yield return (hasProp);
          yield return (hasProp.Assign(false));

          separator = new InlineCode(
            If.Then(
              hasProp.EqualTo(false),
              Expression.Block(jsonWriter.WriteValueSeparator(),
                hasProp.Assign(true))
            ));
        }

        var sortedProperties = alwaysWrittenProperties.Concat(conditionalProperties).ToArray();

        yield return convertPropertyToCode(sortedProperties[0], null);

        foreach (var property in sortedProperties.Skip(1))
          yield return convertPropertyToCode(property, separator);
      }

      return new InlineCode(renderNode());
    }

    static IEnumerable<NodeProperty> GetNodeProperties(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      MemberAccess<Func<object,string>> uriGenerator,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
      Type resourceType,
      TypedExpression<string> resourceUri,
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
        yield return WriteId(jsonWriter, resourceUri);
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
            jsonFormatterResolver,
            resource);

          yield return nodePropertyValue;
          continue;
        }

        yield return WriteNodeProperty(
          jsonWriter, resource, uriGenerator, models, recursionDefender, pi,
          jsonFormatterResolver);
        ;
      }

      foreach (var link in model.Links)
      {
        yield return WriteNodeLink(jsonWriter, link.Relationship, link.Uri, resourceUri, link);
      }
    }

    static NodeProperty WriteNodeLink(
      Variable<JsonWriter> jsonWriter,
      string linkRelationship,
      Uri linkUri,
      TypedExpression<string> resourceUri,
      ResourceLinkModel link)
    {
      IEnumerable<AnyExpression> getNodeLink()
      {
        yield return jsonWriter.WritePropertyName(linkRelationship);
        yield return jsonWriter.WriteBeginObject();
        yield return jsonWriter.WriteRaw(Nodes.IdProperty);

        var uriCombinationMethodInfo = link.CombinationType == ResourceLinkCombination.SubResource
          ? typeof(HydraTextExtensions).GetMethod(nameof(HydraTextExtensions.UriSubResourceCombine),
            BindingFlags.Static | BindingFlags.NonPublic)
          : typeof(HydraTextExtensions).GetMethod(nameof(HydraTextExtensions.UriStandardCombine),
            BindingFlags.Static | BindingFlags.NonPublic);

        var uriCombine = Expression.Call(
          uriCombinationMethodInfo, 
          resourceUri,
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
      MemberAccess<Func<object,string>> uriGenerator,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      PropertyInfo pi,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver
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
              uriGenerator, models, jsonFormatterResolver, recursionDefender: recursionDefender),
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
        var propValue = WriteNodePropertyValue(jsonWriter, pi, jsonFormatterResolver, resource);
        propValue.Preamble = preamble;
        propValue.Conditional = Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType));
        return propValue;
      }

      // it's a list of nodes
      return WriteNodeList(jsonWriter, uriGenerator, models, recursionDefender, pi, jsonFormatterResolver,
        itemResourceRegistrations, propertyValue, preamble);
    }

    static NodeProperty WriteNodeList(
      Variable<JsonWriter> jsonWriter,
      MemberAccess<Func<object,string>> uriGenerator,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      PropertyInfo pi,
      Variable<HydraJsonFormatterResolver> resolver,
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
          uriGenerator,
          models,
          resolver, 
          recursionDefender: recursionDefender);
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
      TypedExpression<string> uri)
    {
      return new NodeProperty("@id")
      {
        Code = new InlineCode(jsonWriter.WriteRaw(Nodes.IdProperty), jsonWriter.WriteString(uri))
      };
    }

    static NodeProperty WriteContext(
      Variable<JsonWriter> jsonWriter,
      TypedExpression<string> uri)
    {
      var writeContextProperty = jsonWriter.WriteRaw(Nodes.ContextProperty);
      var writeContextUri = jsonWriter.WriteString(uri);
      return new NodeProperty("@context")
      {
        Code = new InlineCode(
          writeContextProperty,
          writeContextUri)
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

    static IEnumerable<Expression> WriteBeginObjectContext(Variable<JsonWriter> jsonWriter, TypedExpression<string> contextUri)
    {
      yield return jsonWriter.WriteRaw(Nodes.BeginObjectContext);
      yield return jsonWriter.WriteString(contextUri);
    }

    static NodeProperty WriteNodePropertyValue(
      Variable<JsonWriter> jsonWriter,
      PropertyInfo pi,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
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
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
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