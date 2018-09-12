namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class Operation : JsonLd.IBlankNode
  {
    public string Method { get; set; }
    public string Expects { get; set; }
    public string Title { get; set; }
  }
}