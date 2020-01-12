namespace Tests.Plugins.Hydra.wf
{
  public class Variable
  {
    public string Scheme { get; set; }
    public string Taxonomy { get; set; }
    public string Classification { get; set; }
    public string Provider { get; set; }
    public string Source { get; set; }
    public string Name { get; set; }
    public Provenance Provenance { get; set; }
    public int Tally { get; set; }
    public decimal Coverage { get; set; }
    public bool Ghost { get; set; }
    public int Multiplicity { get; set; }
    public MonetaryAmount MinPrice { get; set; }
    public MonetaryAmount MaxPrice { get; set; }
  }
}
