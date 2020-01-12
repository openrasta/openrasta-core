using System;
using System.Collections.Generic;
using System.IO;
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
    public static CodeBlock ResourceDocument(CompilerContext compilerContext, Variable<JsonWriter> jsonWriter,
      Expression resource,
      Variable<SerializationContext> options)
    {
      var uriGenerator = options.get_UriGenerator();
      var typeGenerator = options.get_TypeGenerator();
      var baseUri = options.get_BaseUri().ObjectToString();
      var contextUri = StringMethods.Concat(baseUri, New.Const(".hydra/context.jsonld"));

      var jsonFormatterResolver = New.Var<HydraJsonFormatterResolver>("resolver");
      var assignResolver = jsonFormatterResolver.Assign(New.Instance<HydraJsonFormatterResolver>());

      var rootNode = WriteNode(
        compilerContext,
        jsonWriter,
        baseUri,
        resource,
        uriGenerator,
        typeGenerator,
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

    static CodeBlock WriteNode(CompilerContext compilerContext, Variable<JsonWriter> jsonWriter,
      TypedExpression<string> baseUri,
      Expression resource,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
      NodeProperty[] existingNodeProperties = null,
      Stack<ResourceModel> recursionDefender = null)
    {
      recursionDefender = recursionDefender ?? new Stack<ResourceModel>();
      var resourceType = compilerContext.Resource.ResourceType;

      if (recursionDefender.Contains(compilerContext.Resource))
        throw new InvalidOperationException(
          $"Detected recursion, already processing {resourceType?.Name}: {string.Join("->", recursionDefender.Select(m => m.ResourceType?.Name).Where(n => n != null))}");

      recursionDefender.Push(compilerContext.Resource);

      var resourceUri = uriGenerator.Invoke(resource);

      List<NodeProperty> nodeProperties =
        new List<NodeProperty>(existingNodeProperties ?? Enumerable.Empty<NodeProperty>());

      nodeProperties.AddRange(GetNodeProperties(compilerContext,
        jsonWriter,
        baseUri,
        resource,
        uriGenerator,
        typeGenerator,
        recursionDefender,
        jsonFormatterResolver,
        resourceUri));


      IEnumerable<AnyExpression> render()
      {
        if (compilerContext.Resource.Hydra().Collection.IsCollection && !compilerContext.Resource.Hydra().Collection.IsHydraCollectionType)
        {
          var collectionItemType = compilerContext.Resource.Hydra().Collection.ItemType;
          var hydraCollectionType = HydraTypes.Collection.MakeGenericType(collectionItemType);
          var collectionCtor =
            hydraCollectionType.GetConstructor(new[]
              {typeof(IEnumerable<>).MakeGenericType(collectionItemType), typeof(string)});
          var collectionWrapper = Expression.Variable(hydraCollectionType, "hydraCollectionType");

          yield return collectionWrapper;
          var instantiateCollection = Expression.Assign(
            collectionWrapper,
            Expression.New(collectionCtor, resource, Expression.Constant(compilerContext.Resource.Hydra().Collection.ManagesRdfTypeName)));
          yield return (instantiateCollection);

          resource = collectionWrapper;

          var hydraCollectionModel = compilerContext.MetaModel.GetResourceModel(hydraCollectionType);

          var hydraCollectionProperties = GetNodeProperties(
            compilerContext.Push(hydraCollectionModel),
            jsonWriter,
            baseUri,
            collectionWrapper,
            uriGenerator,
            typeGenerator,
            recursionDefender,
            jsonFormatterResolver,
            resourceUri).ToList();

          var nodeTypeNode = nodeProperties.FirstOrDefault(x => x.Name == "@type");

          if (nodeTypeNode != null && compilerContext.Resource.Hydra().Collection.IsFrameworkCollection)
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

        var sortedProperties = alwaysWrittenProperties.Concat(conditionalProperties).ToArray();

        // tricky code ahead. If there are always written props, we optimise by simply always writing the separator
        // except for the first.
        // If not, we generate the tracking var.
        if (alwaysWrittenProperties.Any())
        {
          for (var index = 0; index < sortedProperties.Length; index++)
          {
            yield return sortedProperties[index].ToCode(index == 0
              ? null
              : new InlineCode(jsonWriter.WriteValueSeparator()));
          }
        }
        else
        {
          var propAlreadyWritten = New.Var<bool>("propAlreadyWritten");
          yield return propAlreadyWritten;
          yield return propAlreadyWritten.Assign(false);

          separator = new InlineCode(
            If.ThenElse(
              propAlreadyWritten.EqualTo(true),
              Expression.Block(jsonWriter.WriteValueSeparator()),
              propAlreadyWritten.Assign(true))
          );
          foreach (var nodeProp in sortedProperties)
            yield return nodeProp.ToCode(separator);
        }
      }

      return new InlineCode(renderNode());
    }

    static IEnumerable<NodeProperty> GetNodeProperties(CompilerContext compilerContext, Variable<JsonWriter> jsonWriter,
      TypedExpression<string> baseUri,
      Expression resource,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
      Stack<ResourceModel> recursionDefender,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver,
      TypedExpression<string> resourceUri)
    {
      var propNames = compilerContext.Resource
        .Hydra().ResourceProperties
        .Select(p => p.Name)
        .ToArray();

      var generatedIdNode =
        propNames.All(name => name != "@id") && compilerContext.Resource.Uris.Any()
          ? new[] {WriteId(jsonWriter, resourceUri)}
          : Enumerable.Empty<NodeProperty>();

      var generatedTypeNode = propNames.Any(name => name == "@type")
        ? Enumerable.Empty<NodeProperty>()
        : new[]
        {
          compilerContext.Resource.Hydra().JsonLdTypeFunc != null
            ? WriteType(jsonWriter, typeGenerator.Invoke(resource))
            : WriteType(jsonWriter, compilerContext.Resource.Hydra().JsonLdType)
        };

      var linkNodes = compilerContext.Resource.Links
        .Select(link => WriteNodeLink(jsonWriter, link.Relationship, link.Uri, resourceUri, link));

      var valueNodes = compilerContext.Resource.Hydra()
        .ResourceProperties
        .Where(resProperty => resProperty.IsValueNode)
        .Select(resProperty => CreateNodePropertyValue(jsonWriter, resProperty, jsonFormatterResolver, resource))
        .ToList();

      var propNodes = compilerContext.Resource.Hydra()
        .ResourceProperties
        .Where(resProperty => !resProperty.IsValueNode)
        .Select(resProperty => CreateNodeProperty(compilerContext, jsonWriter, baseUri, resource, uriGenerator,
          typeGenerator,
          recursionDefender,
          resProperty, jsonFormatterResolver))
        .ToList();

      return generatedIdNode.Concat(generatedTypeNode).Concat(propNodes).Concat(valueNodes).Concat(linkNodes).ToList();
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

    static NodeProperty CreateNodeProperty(CompilerContext compilerContext, Variable<JsonWriter> jsonWriter,
      TypedExpression<string> baseUri,
      Expression resource,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
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

      if (compilerContext.MetaModel.TryGetResourceModel(pi.PropertyType, out var propertyResourceModel))
      {
        var jsonPropertyName = property.Name;
        return new NodeProperty(jsonPropertyName)
        {
          Preamble = preamble,
          Code = new InlineCode(new[]
          {
            jsonWriter.WritePropertyName(jsonPropertyName),
            jsonWriter.WriteBeginObject(),
            WriteNode(compilerContext.Push(propertyResourceModel), jsonWriter, baseUri, propertyValue,
              uriGenerator, typeGenerator, jsonFormatterResolver, recursionDefender: recursionDefender),
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
        let resourceModels = compilerContext.MetaModel.ResourceRegistrations.Where(r =>
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

      if (itemResourceRegistrations.Any())
        return CreateNodePropertyAsList(compilerContext, jsonWriter, baseUri, uriGenerator, typeGenerator,
          recursionDefender,
          property,
          jsonFormatterResolver,
          itemResourceRegistrations, propertyValue, preamble);

      // not a list of iri or blank nodes
      var propValue = CreateNodePropertyValue(jsonWriter, property, jsonFormatterResolver, resource);
      propValue.Preamble = preamble;
      propValue.Conditional = Expression.NotEqual(propertyValue, Expression.Default(pi.PropertyType));
      return propValue;

      // it's a list of nodes
    }

    static NodeProperty CreateNodePropertyAsList(CompilerContext compilerContext, Variable<JsonWriter> jsonWriter,
      TypedExpression<string> baseUri,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
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
        Expression.Call(Reflection.Enumerable.Any.MakeGenericMethod(itemRegistration.itemType), propertyValue);


      var itemArrayType = itemRegistration.itemType.MakeArrayType();

      var itemArray = Expression.Variable(itemArrayType, "itemArray");
      var itemArrayAssignment = Expression.Assign(
        itemArray,
        Expression.Call(Reflection.Enumerable.ToArray.MakeGenericMethod(itemRegistration.itemType), propertyValue));

      var currentArrayIndex = New.Var<int>("currentArrayIndex");
      var currentArrayElement = Expression.ArrayAccess(itemArray, currentArrayIndex);

      var renderBlock = WriteNodeWithInheritanceChain(compilerContext, jsonWriter, uriGenerator, typeGenerator,
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

    static InlineCode WriteNodeWithInheritanceChain(CompilerContext compilerContext, Variable<JsonWriter> jsonWriter,
      MemberAccess<Func<object, string>> uriGenerator,
      MemberAccess<Func<object, string>> typeGenerator,
      Stack<ResourceModel> recursionDefender,
      Variable<HydraJsonFormatterResolver> resolver,
      (Type itemType, List<ResourceModel> models) itemRegistration,
      Expression resource,
      TypedExpression<string> baseUri)
    {
      CodeBlock resourceBlock(ResourceModel r, ParameterExpression typed)
      {
        return WriteNode(
          compilerContext.Push(r),
          jsonWriter, 
          baseUri,
          typed,
          uriGenerator, typeGenerator,
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
          new InlineCode(jsonWriter.WriteRaw(Nodes.TypeProperty), jsonWriter.WriteStringRaw(type))
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

    static NodeProperty CreateNodePropertyValue(
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
        var resolverGetFormatter = Reflection.HydraJsonFormatterResolver.GetFormatter.MakeGenericMethod(propertyType);
        var jsonFormatterType = typeof(IJsonFormatter<>).MakeGenericType(propertyType);
        var serializeMethod = jsonFormatterType.GetMethod("Serialize",
          new[] {typeof(JsonWriter).MakeByRefType(), propertyType, typeof(IJsonFormatterResolver)});

        var formatterInstance = Expression.Variable(jsonFormatterType, "formatter");
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