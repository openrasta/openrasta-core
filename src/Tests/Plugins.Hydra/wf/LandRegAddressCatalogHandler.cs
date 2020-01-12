namespace Tests.Plugins.Hydra.wf
{
  public class LandRegAddressCatalogHandler
  {
    public LandRegAddressCatalog Get(string resourceIdentifier)
    {
      var c = new LandRegAddressCatalog()
      {
        CatalogType = "address",
        ResourceIdentifier = resourceIdentifier,
 
      };
      c.Add(new LandRegDatum("kjsdfhkjfsd0","kfdsjkhfsd", "ksjdhfkjhs","name")
      {
        Variable = new Variable()
        {
          Scheme = "scheme",
          Taxonomy = "blah",
          Classification = "blah",
          Name = "foff",
          Provider = "prov",
          Source = "source"
        },
        Price = new MonetaryAmount(0.1m,"GBP"),
        Value = "12"
      });
      return c;
    }
  }
}