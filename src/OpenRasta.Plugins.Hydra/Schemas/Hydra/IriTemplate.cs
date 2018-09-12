using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class IriTemplate : JsonLd.IBlankNode
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