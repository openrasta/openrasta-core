using System.Collections.Generic;
using Newtonsoft.Json;
using OpenRasta.OperationModel;
using OpenRasta.Plugins.Hydra.Internal;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class ApiDocumentation : IIriNode
  {
    [JsonProperty("supportedClass")]
    public Class[] SupportedClasses { get; set; }
  }
}