using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class IriTemplateMapping
  {
    public IriTemplateMapping(string variable)
    {
      Variable = variable;
    }

    public string Variable { get; set; }
    public string Property { get; set; } = "hydra:freetextQuery";
    public bool Required { get; set; } = true;
  }
}