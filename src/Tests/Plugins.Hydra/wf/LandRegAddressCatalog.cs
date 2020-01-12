using System.Collections.Generic;
using OpenRasta.Web;

namespace Tests.Plugins.Hydra.wf
{
  public class LandRegAddressCatalog : List<LandRegDatum>
  {
    public LandRegAddressCatalog()
    {
      ProcessingFee = new MonetaryAmount(0.01m, "GBP");
    }
    //
    // public string ResourceType { get; set; }
    public string ResourceIdentifier { get; set; }
    public string CatalogType { get; set; }
    public MonetaryAmount ProcessingFee { get; set; }
  }
}