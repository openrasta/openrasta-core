using System.Collections.Generic;

namespace OpenRasta.Plugins.Hydra.Schemas
{
  public static partial class HydraCore
  {
    public class IriTemplate //: JsonLd.IBlankNode
    {
      public IriTemplate(string template)
      {
        Template = template;
      }

      public string Template { get; set; }

      public string VariableRepresentation { get; set; } = "BasicRepresentation";

      public List<IriTemplateMapping> Mapping { get; set; } = new List<IriTemplateMapping>();
    }
  }
}