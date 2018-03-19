namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class WeakETagValidator : ETagValidator
  {
    public WeakETagValidator(string value)
    {
      Value = value;
    }

    string Value { get; }

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