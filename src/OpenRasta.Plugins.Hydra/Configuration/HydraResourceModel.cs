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
    public List<Operation> SupportedOperations { get; set; } = new List<Operation>();
    public Class ApiClass { get; set; }
    public Func<object, string> TypeFunc { get; set; }
    public List<ResourceProperty> ResourceProperties { get; set; }
  }
}