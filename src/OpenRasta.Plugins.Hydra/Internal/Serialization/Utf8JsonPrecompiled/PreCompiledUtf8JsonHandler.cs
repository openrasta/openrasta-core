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
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class PreCompiledUtf8JsonHandler : IMetaModelHandler
  {
    public void PreProcess(IMetaModelRepository repository)
    {
      // EnsureNoDuplicateRegistrations(repository);

      foreach (var model in repository.ResourceRegistrations.Where(r => r.ResourceType != null).ToList())
        AnnotateCollectionTypes(repository, model);

      foreach (var model in repository.ResourceRegistrations.Where(r => r.ResourceType != null).ToList())
        AddImplicitCollectionRegistrations(repository, model);

      foreach (var model in repository.ResourceRegistrations.Where(r => r.ResourceType != null).ToList())
        PrepareProperties(model);


      foreach (var model in repository.ResourceRegistrations
        .Where(r => r.ResourceType != null   && r.Hydra().Vocabulary  != null)
        .ToList())
        CreateClass(model);

      // EnsureNoDuplicateRegistrations(repository);
    }

    static void CreateClass(ResourceModel model)
    {
      var hydraModel = model.Hydra();
      var hydraClass = hydraModel.Class ?? (hydraModel.Class = new Class());

      var vocabPrefix = hydraModel.Vocabulary.DefaultPrefix;
      var className = model.ResourceType.Name;
      var identifier = vocabPrefix != null ? $"{vocabPrefix}:{className}" : className;


      hydraModel.Class = new Class
      {
        Identifier = identifier,
        SupportedProperties = hydraModel.ResourceProperties.Select(p =>
          new SupportedProperty()
          {
            Property = new Rdf.Property()
            {
              Identifier = $"{identifier}/{p.Name}",
              Range = p.RdfRange
            }
          }
        ).ToList(),
        SupportedOperations = hydraModel.SupportedOperations 
      };
    }

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
      if (hydra.Collection.IsCollection || typeof(Collection).IsAssignableFrom(model.ResourceType)) return;

      var collectionRm = new ResourceModel
      {
        ResourceKey = typeof(Schemas.Hydra.Hydra.Collection<>).MakeGenericType(model.ResourceType)
      };


      var collectionHydra = collectionRm.Hydra();
      collectionHydra.Vocabulary = Vocabularies.Hydra;
      collectionHydra.TypeFunc = _ => "hydra:Collection";

      repository.ResourceRegistrations.Add(collectionRm);
    }

    void AnnotateCollectionTypes(IMetaModelRepository repository, ResourceModel model)
    {
      var enumerableTypes = HydraTextExtensions.IEnumerableItemTypes(model.ResourceType).ToList();

      var enumerableType = enumerableTypes.FirstOrDefault();
      if (enumerableType != null && repository.TryGetResourceModel(enumerableType, out var itemModel))
      {
        var hydraResourceModel = model.Hydra();
        hydraResourceModel.Collection.IsCollection = true;
        hydraResourceModel.Collection.ItemType = itemModel.ResourceType;
        hydraResourceModel.Collection.IsFrameworkCollection =
          enumerableType == model.ResourceType || (model.ResourceType.IsGenericType &&
                                                   model.ResourceType.GetGenericTypeDefinition() == typeof(List<>));
      }
    }

    void PrepareProperties(ResourceModel model)
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
          Name = HydraTextExtensions.GetJsonPropertyName(pi),
          IsValueNode = pi.PropertyType == typeof(string) ||
                        (pi.PropertyType.IsValueType && Nullable.GetUnderlyingType(pi.PropertyType) == null),
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
        hydraResourceModel.SerializeFunc = model.ResourceType == typeof(Context)
          ? CreateContextSerializer()
          : CreateDocumentSerializer(model, repository);
        hydraResourceModel.ManagesBlockType = GetTypeName(repository, model);
      }
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

    Func<object, SerializationContext, Stream, Task> CreateContextSerializer()
    {
      // Hack. 3am. meh.
      return (o, context, stream) =>
        JsonSerializer.SerializeAsync(stream, (Context) o, new HydraJsonFormatterResolver());
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