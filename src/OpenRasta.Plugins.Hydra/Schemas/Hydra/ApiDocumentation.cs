using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas
{
  public static partial class HydraCore
  {
    public class ApiDocumentation
    {
      [JsonProperty("supportedClass")]
      public Class[] SupportedClasses { get; set; }
    }
  }
}