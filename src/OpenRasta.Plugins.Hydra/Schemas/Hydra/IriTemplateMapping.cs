namespace OpenRasta.Plugins.Hydra.Schemas
{

  public static partial class HydraCore
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
}