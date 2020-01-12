using System;

namespace Tests.Plugins.Hydra.wf
{
  public class LandRegDatum : LandRegVariableType
  {
    public LandRegDatum(string scheme, string taxonomy, string classification, string name)
       : base(scheme, taxonomy, classification, name)
    {
    }

    public MonetaryAmount Price { get; set; }
    public Variable Variable { get; set; }
    public string Value { get; set; }
  }
}