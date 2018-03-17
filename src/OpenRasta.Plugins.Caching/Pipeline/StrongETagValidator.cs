namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class StrongETagValidator : ETagValidator
  {
    public StrongETagValidator(string value)
    {
      Value = value;
    }

    public string Value { get; }

    public override bool Matches(string entityTag)
    {
      return entityTag == Value;
    }

    public override string ToString()
    {
      return Value;
    }
  }
}