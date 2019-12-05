using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas;
using OpenRasta.Plugins.Hydra.Internal.Serialization;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;


namespace OpenRasta.Plugins.Hydra.Configuration
{
  public class HydraResourceModel
  {
    public ResourceModel ResourceModel { get; }

    public HydraResourceModel(ResourceModel resourceModel)
    {
      ResourceModel = resourceModel;
      Collection = new CollectionModel(this);
    }

    public Func<object, SerializationContext, Stream, Task> SerializeFunc { get; set; }
    public Vocabulary Vocabulary { get; set; }
    public HydraCore.Class Class { get; set; }
    public Func<object, string> TypeFunc { get; set; }

    public List<ResourceProperty> ResourceProperties { get; } = new List<ResourceProperty>();

    // public string ManagesBlockType { get; set; }
    public CollectionModel Collection { get; }
    public List<HydraCore.Operation> SupportedOperations { get; } = new List<HydraCore.Operation>();
    public string TypeName { get; set; }

    public class CollectionModel
    {
      public HydraResourceModel ResourceHydraModel { get; }

      public CollectionModel(HydraResourceModel resource)
      {
        ResourceHydraModel = resource;
      }

      public bool IsCollection { get; set; }
      public Type ItemType => ItemModel?.ResourceType;
      public bool IsFrameworkCollection { get; set; }
      public string ManagesRdfTypeName => ItemModel?.Hydra().TypeName;

      public bool IsHydraCollectionType => typeof(HydraCore.Collection)
        .IsAssignableFrom(ResourceHydraModel.ResourceModel.ResourceType);

      public ResourceModel ItemModel { get; set; }
    }
  }
}