namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class WeakETagValidator : ETagValidator
  {
    public WeakETagValidator(string value)
    {
      Value = value;
    }

    public string Value { get; set; }

    public override bool Matches(string entityTag)
    {
      // TODO: We ignore weak etags alltogether for now
      return false;
    }

    public override string ToString()
    {
      return "W/" + Value;
    }
  }
}