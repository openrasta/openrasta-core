using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenRasta.Plugins.Hydra.Internal;
using OpenRasta.Plugins.Hydra.Internal.Serialization;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Configuration
{
  public class HydraResourceModel
  {
    public Func<object,SerializationContext, Stream,Task> SerializeFunc { get; set; }
    public Vocabulary Vocabulary { get; set; }
    public Class Class { get; set; }
    public Func<object, string> TypeFunc { get; set; }
    public List<ResourceProperty> ResourceProperties { get; } = new List<ResourceProperty>();
    public string ManagesBlockType { get; set; }
    public CollectionModel Collection { get; } = new CollectionModel();
    public List<Operation> SupportedOperations { get; } = new List<Operation>();

    public class CollectionModel
    {
      public bool IsCollection { get; set; }
      public Type ItemType { get; set; }
      public bool IsFrameworkCollection { get; set; }
    }
  }
}