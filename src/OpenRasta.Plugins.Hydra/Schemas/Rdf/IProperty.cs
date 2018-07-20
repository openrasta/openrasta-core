using System.Reflection;

namespace OpenRasta.Plugins.Hydra.Schemas
{
  public static partial class Rdf
  {
    public interface IProperty
    {
      PropertyInfo PropertyInfo { get; set; }
    }
  }
}