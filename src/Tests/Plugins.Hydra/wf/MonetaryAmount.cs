namespace Tests.Plugins.Hydra.wf
{
  public class MonetaryAmount
  {
    public MonetaryAmount(decimal value, string currency)
    {
      Value = value;
      Currency = currency;
    }

    public MonetaryAmount()
    {

    }

    public decimal Value { get; set; }
    public string Currency { get; set; }
  }
}