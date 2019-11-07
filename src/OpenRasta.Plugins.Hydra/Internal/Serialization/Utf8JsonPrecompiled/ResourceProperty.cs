using System.Reflection;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class ResourceProperty
  {
    public PropertyInfo Member { get; set; }
    public string Name { get; set; }
    public bool IsValueNode { get; set; }
  }
}