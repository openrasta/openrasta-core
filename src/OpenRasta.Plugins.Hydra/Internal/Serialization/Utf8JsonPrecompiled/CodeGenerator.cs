using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;
using OpenRasta.TypeSystem.ReflectionBased;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class CodeGenerator
  {
    static readonly MethodInfo ResolverGetFormatterMethodInfo =
      typeof(HydraJsonFormatterResolver).GetMethod(nameof(IJsonFormatterResolver.GetFormatter));

    static readonly MethodInfo EnumerableToArrayMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));

    static readonly MethodInfo EnumerableAnyMethodInfo =
      typeof(Enumerable).GetMethods().Single(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 1);

    public static CodeBlock ResourceDocument(
      Variable<JsonWriter> jsonWriter,
      ResourceModel model,
      Expression resource,
      Variable<SerializationContext> options,
      IMetaModelRepository models)
    {
      var uriGenerator = options.get_UriGenerator();
      var typeGenerator = options.get_TypeGenerator();
      var baseUri = options.get_BaseUri().ObjectToString();
      var contextUri = StringMethods.Concat(baseUri, New.Const(".hydra/context.jsonld"));

      var jsonFormatterResolver = New.Var<HydraJsonFormatterResolver>("resolver");
      var assignResolver = jsonFormatterResolver.Assign(New.Instance<HydraJsonFormatterResolver>());

      var rootNode = WriteNode(
        jsonWriter,
        baseUri,
        model,
        resource,
        uriGenerator,
        typeGenerator,
        models,
        jsonFormatterResolver,
        new[]
        {
          WriteContext(jsonWriter, contextUri)
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
      TypedExpression<string> baseUri,
      ResourceModel model,
      Expression resource,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
      IMetaModelRepository models,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
      NodeProperty[] existingNodeProperties = null,
      Stack<ResourceModel> recursionDefender = null)
    {
      recursionDefender = recursionDefender ?? new Stack<ResourceModel>();
      var resourceType = model.ResourceType;

      if (recursionDefender.Contains(model))
        throw new InvalidOperationException(
          $"Detected recursion, already processing {resourceType?.Name}: {string.Join("->", recursionDefender.Select(m => m.ResourceType?.Name).Where(n => n != null))}");

      recursionDefender.Push(model);

      var resourceRegistrationHydraType = model.Hydra().JsonLdType;
      var resourceUri = uriGenerator.Invoke(resource);

      List<NodeProperty> nodeProperties =
        new List<NodeProperty>(existingNodeProperties ?? Enumerable.Empty<NodeProperty>());

      nodeProperties.AddRange(GetNodeProperties(
        jsonWriter,
        baseUri,
        model,
        resource,
        uriGenerator,
        typeGenerator,
        models,
        recursionDefender,
        jsonFormatterResolver,
        resourceType,
        resourceUri,
        resourceRegistrationHydraType));


      IEnumerable<AnyExpression> render()
      {
        if (model.Hydra().Collection.IsCollection && !model.Hydra().Collection.IsHydraCollectionType)
        {
          var collectionItemType = model.Hydra().Collection.ItemType;
          var hydraCollectionType = HydraTypes.Collection.MakeGenericType(collectionItemType);
          var collectionCtor =
            hydraCollectionType.GetConstructor(new[]
              {typeof(IEnumerable<>).MakeGenericType(collectionItemType), typeof(string)});
          var collectionWrapper = Expression.Variable(hydraCollectionType);

          yield return collectionWrapper;
          var instantiateCollection = Expression.Assign(
            collectionWrapper,
            Expression.New(collectionCtor, resource, Expression.Constant(model.Hydra().Collection.ManagesRdfTypeName)));
          yield return (instantiateCollection);

          resource = collectionWrapper;

          var hydraCollectionModel = models.GetResourceModel(hydraCollectionType);

          var hydraCollectionProperties = GetNodeProperties(
            jsonWriter,
            baseUri,
            hydraCollectionModel,
            collectionWrapper,
            uriGenerator,
            typeGenerator,
            models,
            recursionDefender,
            jsonFormatterResolver,
            resourceType,
            resourceUri,
            resourceRegistrationHydraType).ToList();

          var nodeTypeNode = nodeProperties.FirstOrDefault(x => x.Name == "@type");

          if (nodeTypeNode != null && model.Hydra().Collection.IsFrameworkCollection)
          {
            nodeProperties.Clear();
            nodeProperties.AddRange(hydraCollectionProperties);
          }
          else
          {
            hydraCollectionProperties.RemoveAll(n => n.Name == "@type");
            nodeProperties.AddRange(hydraCollectionProperties);
          }
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

          yield return hasProp;
          yield return hasProp.Assign(false);

          separator = new InlineCode(
            If.Then(
              hasProp.EqualTo(false),
              Expression.Block(jsonWriter.WriteValueSeparator(),
                hasProp.Assign(true))
            ));
        }

        var sortedProperties = alwaysWrittenProperties.Concat(conditionalProperties).ToArray();

        for (var index = 0; index < sortedProperties.Length; index++)
        {
          yield return sortedProperties[index].ToCode(index == 0 ? null : separator);
        }
      }

      return new InlineCode(renderNode());
    }

    static IEnumerable<NodeProperty> GetNodeProperties(
      Variable<JsonWriter> jsonWriter,
      TypedExpression<string> baseUri,
      ResourceModel model,
      Expression resource,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
      Type resourceType,
      TypedExpression<string> resourceUri,
      string resourceRegistrationHydraType)
    {
      var propNames = model
        .Hydra().ResourceProperties
        .Select(p => p.Name);

      var overridesId = propNames.Any(name => name == "@id");
      var overridesType = propNames.Any(name => name == "@type");

      if (overridesId == false && model.Uris.Any())
      {
        yield return WriteId(jsonWriter, resourceUri);
      }

      if (overridesType == false)
      {
        var typePropertyFactory = model.Hydra().TypeFunc;
        if (typePropertyFactory == null)
        {
          yield return WriteType(jsonWriter, resourceRegistrationHydraType);
        }
        else
        {
          yield return WriteType(jsonWriter, typeGenerator.Invoke(resource));
        }
      }


      foreach (var resourceProperty in model.Hydra().ResourceProperties)
      {
        if (resourceProperty.IsValueNode)
        {
          var nodePropertyValue = WriteNodePropertyValue(
            jsonWriter,
            resourceProperty,
            jsonFormatterResolver,
            resource);

          yield return nodePropertyValue;
          continue;
        }

        yield return WriteNodeProperty(
          jsonWriter, baseUri, resource, uriGenerator, typeGenerator, models, recursionDefender, resourceProperty,
          jsonFormatterResolver);
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

        var uriCombine = new MethodCall<string>(Expression.Call(
          uriCombinationMethodInfo,
          resourceUri,
          Expression.Constant(linkUri, typeof(Uri))));

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
      TypedExpression<string> baseUri,
      Expression resource,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      ResourceProperty property,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver)
    {
      var pi = property.Member;
      // var propertyValue;
      var propertyValue = Expression.Variable(pi.PropertyType, $"val{pi.DeclaringType.Name}{pi.Name}");

      // propertyValue = resource.Property;
      var propertyValueAssignment = Expression.Assign(propertyValue, Expression.MakeMemberAccess(resource, pi));

      var preamble = new InlineCode(new Expression[] {propertyValue, propertyValueAssignment});

      if (models.TryGetResourceModel(pi.PropertyType, out var propertyResourceModel))
      {
        var jsonPropertyName = property.Name;
        return new NodeProperty(jsonPropertyName)
        {
          Preamble = preamble,
          Code = new InlineCode(new[]
          {
            jsonWriter.WritePropertyName(jsonPropertyName),
            jsonWriter.WriteBeginObject(),
            WriteNode(jsonWriter, baseUri, propertyResourceModel, propertyValue,
              uriGenerator, typeGenerator, models, jsonFormatterResolver, recursionDefender: recursionDefender),
            jsonWriter.WriteEndObject()
          }),
          Conditional = Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType))
        };
      }

      var itemTypes = (from i in pi.PropertyType.GetInterfaces()
          .Concat(pi.PropertyType.IsInterface ? new[] {pi.PropertyType} : Array.Empty<Type>())
        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        let itemType = i.GetGenericArguments()[0]
        where itemType != typeof(object)
        select itemType).ToList();

      // not an iri node itself, but is it a list of nodes?
      var itemResourceRegistrations = (
        from itemType in itemTypes
        let resourceModels = models.ResourceRegistrations.Where(r =>
          r.ResourceType != null && itemType.IsAssignableFrom(r.ResourceType)).ToList()
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
        var propValue = WriteNodePropertyValue(jsonWriter, property, jsonFormatterResolver, resource);
        propValue.Preamble = preamble;
        propValue.Conditional = Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType));
        return propValue;
      }

      // it's a list of nodes
      return WriteNodePropertyAsList(jsonWriter, baseUri, uriGenerator, typeGenerator, models, recursionDefender,
        property,
        jsonFormatterResolver,
        itemResourceRegistrations, propertyValue, preamble);
    }

    static NodeProperty WriteNodePropertyAsList(
      Variable<JsonWriter> jsonWriter,
      TypedExpression<string> baseUri,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      ResourceProperty property,
      Variable<HydraJsonFormatterResolver> resolver,
      List<(Type itemType, List<ResourceModel> models)> itemResourceRegistrations,
      ParameterExpression propertyValue,
      InlineCode preamble)
    {
      var jsonPropertyName = property.Name;

      var itemRegistration = itemResourceRegistrations.First();

      var conditionalEnumerableAny =
        Expression.Call(EnumerableAnyMethodInfo.MakeGenericMethod(itemRegistration.itemType), propertyValue);


      var itemArrayType = itemRegistration.itemType.MakeArrayType();

      var itemArray = Expression.Variable(itemArrayType);
      var itemArrayAssignment = Expression.Assign(
        itemArray,
        Expression.Call(EnumerableToArrayMethodInfo.MakeGenericMethod(itemRegistration.itemType), propertyValue));

      var currentArrayIndex = New.Var<int>("currentArrayIndex");
      var currentArrayElement = Expression.ArrayAccess(itemArray, currentArrayIndex);

      var renderBlock = WriteNodeWithInheritanceChain(jsonWriter, uriGenerator, typeGenerator, models,
        recursionDefender, resolver,
        itemRegistration, currentArrayElement, baseUri);

      return new NodeProperty(jsonPropertyName)
      {
        Preamble = preamble,
        Code = new InlineCode(new[]
        {
          itemArray,
          itemArrayAssignment,
          currentArrayIndex,
          currentArrayIndex.Assign(0),
          jsonWriter.WritePropertyName(jsonPropertyName),

          jsonWriter.WriteBeginArray(),
          LoopOverArray(jsonWriter, currentArrayIndex, renderBlock, itemArray, itemArrayType),
          jsonWriter.WriteEndArray()
        }),
        Conditional = Expression.AndAlso(
          Expression.NotEqual(propertyValue, Expression.Default(property.Member.PropertyType)),
          conditionalEnumerableAny)
      };
    }

    static LoopExpression LoopOverArray(
      Variable<JsonWriter> jsonWriter,
      Variable<int> currentArrayIndex,
      InlineCode renderBlock,
      ParameterExpression itemArray,
      Type itemArrayType)
    {
      var arrayElement = new InlineCode(
        If.Then(
          currentArrayIndex.GreaterThan(0),
          jsonWriter.WriteValueSeparator()),
        jsonWriter.WriteBeginObject(),
        renderBlock,
        Expression.PostIncrementAssign(currentArrayIndex),
        jsonWriter.WriteEndObject()
      );

      var itemArrayLength = Expression.MakeMemberAccess(itemArray, itemArrayType.GetProperty("Length"));
      var @break = Expression.Label("break");
      var loop = Expression.Loop(
        If.ThenElse(
          currentArrayIndex.LessThan(itemArrayLength),
          arrayElement.ToBlock(),
          Expression.Break(@break)),
        @break
      );
      return loop;
    }

    static InlineCode WriteNodeWithInheritanceChain(
      Variable<JsonWriter> jsonWriter,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender,
      Variable<HydraJsonFormatterResolver> resolver,
      (Type itemType, List<ResourceModel> models) itemRegistration,
      Expression resource,
      TypedExpression<string> baseUri)
    {
      CodeBlock resourceBlock(ResourceModel r, ParameterExpression typed)
      {
        return WriteNode(
          jsonWriter, baseUri,
          r,
          typed,
          uriGenerator, typeGenerator,
          models,
          resolver,
          recursionDefender: recursionDefender);
      }

      {
        // with C : B : A, if is C else if is B else if is A else throw
        Expression codeRender = Expression.Block(Expression.Throw(Expression.New(typeof(InvalidOperationException))));
        var renderBlock = new InlineCode(codeRender);


        foreach (var specificModel in itemRegistration.models)
        {
          var typed = Expression.Variable(specificModel.ResourceType, "as" + specificModel.ResourceType.Name);

          var @as = Expression.Assign(typed,
            Expression.TypeAs(resource, specificModel.ResourceType));

          codeRender = Expression.IfThenElse(
            Expression.NotEqual(@as, Expression.Default(specificModel.ResourceType)),
            resourceBlock(specificModel, @typed),
            codeRender);

          renderBlock = new InlineCode(renderBlock.Variables.Concat(new[] {typed, codeRender}));
        }

        return renderBlock;
      }
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

    static NodeProperty WriteType(Variable<JsonWriter> jsonWriter, string type)
    {
      return new NodeProperty("@type")
      {
        Code =
          new InlineCode(jsonWriter.WriteRaw(Nodes.TypeProperty), jsonWriter.WriteString(type))
      };
    }

    public static NodeProperty WriteType(
      Variable<JsonWriter> jsonWriter,
      TypedExpression<string> type
    )
    {
      return new NodeProperty("@type")
      {
        Code =
          new InlineCode(jsonWriter.WriteRaw(Nodes.TypeProperty), jsonWriter.WriteString(type))
      };
    }

    static NodeProperty WriteNodePropertyValue(
      Variable<JsonWriter> jsonWriter,
      ResourceProperty property,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
      Expression resource)
    {
      var pi = property.Member;
      var propertyGet = Expression.MakeMemberAccess(resource, pi);
      var propertyName = property.Name;
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