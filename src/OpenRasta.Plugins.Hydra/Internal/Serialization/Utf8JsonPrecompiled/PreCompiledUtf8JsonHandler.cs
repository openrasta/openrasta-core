using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;
using OpenRasta.Plugins.Hydra.Schemas;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class PreCompiledUtf8JsonHandler : IMetaModelHandler
  {
    public void PreProcess(IMetaModelRepository repository)
    {
      IterateOverHydraRegistrations(repository,
        AnnotateCollectionTypes,
        AddImplicitCollectionRegistrations,
        AnnotateTypes,
        PrepareProperties,
        CreateClass);
    }

    void AnnotateTypes(IMetaModelRepository repository, ResourceModel model)
    {
      if (model.Hydra().JsonLdType == null)
        model.Hydra().JsonLdType = model.GetJsonLdTypeName();
    }

    void IterateOverHydraRegistrations(
      IMetaModelRepository repo,
      params Action<IMetaModelRepository, ResourceModel>[] handlers)
    {
      foreach (var handler in handlers)
      {
        foreach (var model in repo.ResourceRegistrations
          .Where(r => r.ResourceType != null && r.Hydra().Vocabulary != null)
          .ToList())
          handler(repo, model);
      }
    }

    static void CreateClass(IMetaModelRepository _, ResourceModel model)
    {
      var hydraModel = model.Hydra();
      var hydraClass = hydraModel.Class ?? (hydraModel.Class = new HydraCore.Class());

      var vocabPrefix = hydraModel.Vocabulary.DefaultPrefix;
      var className = model.ResourceType.Name;
      var identifier = vocabPrefix != null ? $"{vocabPrefix}:{className}" : className;


      hydraModel.Class = new HydraCore.Class
      {
        Identifier = identifier,
        SupportedProperties = hydraModel.ResourceProperties
          .Select(p =>
            new HydraCore.SupportedProperty
            {
              Property = new Rdf.Property
              {
                Identifier = $"{identifier}/{p.Name}",
                Range = p.RdfRange
              }
            }
          ).ToList(),
        SupportedOperations = hydraModel.SupportedOperations
      };
    }

    // ReSharper disable once UnusedMember.Local
    static void EnsureNoDuplicateRegistrations(IMetaModelRepository repository)
    {
      var duplicateRegistrations = repository.ResourceRegistrations
        .Where(r => r.ResourceType != null)
        .GroupBy(x => x.ResourceType)
        .Where(byType => byType.Count() > 1)
        .Select(byType => byType.Key)
        .ToList();
      if (duplicateRegistrations.Any())
        throw new InvalidOperationException(
          "Duplicate resource registrations: " + string.Join(", ", duplicateRegistrations));
    }

    void AddImplicitCollectionRegistrations(IMetaModelRepository repository, ResourceModel model)
    {
      var hydra = model.Hydra();
      if (hydra.Collection.IsCollection || hydra.Collection.IsHydraCollectionType) return;

      var collectionRm = new ResourceModel
      {
        ResourceKey = HydraTypes.Collection.MakeGenericType(model.ResourceType)
      };


      var collectionHydra = collectionRm.Hydra();
      collectionHydra.Vocabulary = Vocabularies.Hydra;
      collectionHydra.JsonLdTypeFunc = _ => "hydra:Collection";

      collectionHydra.Collection.IsCollection = true;
      collectionHydra.Collection.IsFrameworkCollection = false;

      repository.ResourceRegistrations.Add(collectionRm);
    }

    void AnnotateCollectionTypes(IMetaModelRepository repository, ResourceModel model)
    {
      var enumerableTypes = model.ResourceType.EnumerableItemTypes().ToList();

      var enumerableType = enumerableTypes.FirstOrDefault();
      if (enumerableType != null && repository.TryGetResourceModel(enumerableType, out var itemModel))
      {
        var hydraResourceModel = model.Hydra();
        hydraResourceModel.Collection.IsCollection = true;
        hydraResourceModel.Collection.ItemModel = itemModel;
        hydraResourceModel.Collection.IsFrameworkCollection =
          enumerableType == model.ResourceType || (model.ResourceType.IsGenericType &&
                                                   model.ResourceType.GetGenericTypeDefinition() == typeof(List<>));
      }
    }

    void PrepareProperties(IMetaModelRepository repository, ResourceModel model)
    {
      if (model.ResourceType == null) return;

      var hydra = model.Hydra();
      var clrProperties = model.ResourceType
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(HydraTextExtensions.IsNotIgnored)
        .Where(pi => pi.GetIndexParameters().Any() == false)
        .Select(pi => new ResourceProperty
        {
          Member = pi,
          Name = pi.GetJsonPropertyName(),
          IsValueNode = pi.PropertyType.IsValueType && Nullable.GetUnderlyingType(pi.PropertyType) == null,
          RdfRange = pi.PropertyType.GetRdfRange()
        })
        .ToList();
      hydra.ResourceProperties.AddRange(clrProperties);
    }

    public void Process(IMetaModelRepository repository)
    {
      foreach (var model in repository.ResourceRegistrations.Where(r => r.ResourceType != null))
      {
        var hydraResourceModel = model.Hydra();

        if (model.ResourceType?.IsGenericTypeDefinition == true) continue;
        hydraResourceModel.SerializeFunc = model.ResourceType == typeof(HydraCore.Context)
          ? CreateContextSerializer()
          : CreateDocumentSerializer(model, repository);
      }
    }


    Func<object, SerializationContext, Stream, Task> CreateContextSerializer()
    {
      // Hack. 3am. meh.
      var hydraJsonFormatterResolver = new HydraJsonFormatterResolver();
      return (o, context, stream) =>
        JsonSerializer.SerializeAsync(stream, (HydraCore.Context) o, hydraJsonFormatterResolver);
    }

    Func<object, SerializationContext, Stream, Task> CreateDocumentSerializer(
      ResourceModel model,
      IMetaModelRepository repository)
    {
      var block = new CodeBlock(RendererBlock(model, repository));

      var lambda =
        Expression.Lambda<Func<object, SerializationContext, Stream, Task>>(
          block,
          "Render",
          block.Parameters);
      return lambda.Compile();
    }

    static IEnumerable<AnyExpression> RendererBlock(
      ResourceModel model,
      IMetaModelRepository repository)
    {
      var resourceIn = New.Parameter<object>("resource");
      var options = New.Parameter<SerializationContext>("options");
      var stream = New.Parameter<Stream>("stream");

      yield return resourceIn;
      yield return options;
      yield return stream;

      var retVal = New.Var<Task>("retVal");

      var resource = Expression.Variable(model.ResourceType, "typedResource");
      yield return resource;

      yield return Expression.Assign(resource, Expression.Convert(resourceIn, model.ResourceType));

      var jsonWriter = New.Var<JsonWriter>("jsonWriter");
      var buffer = New.Var<ArraySegment<byte>>("buffer");

      yield return jsonWriter;
      yield return buffer;

      yield return Expression.Assign(jsonWriter, Expression.New(typeof(JsonWriter)));

      yield return CodeGenerator.ResourceDocument(jsonWriter, model, resource, options, repository);

      yield return Expression.Assign(buffer, jsonWriter.GetBuffer());
      yield return Expression.Assign(retVal, stream.WriteAsync(buffer));
      yield return retVal;
    }
  }
}