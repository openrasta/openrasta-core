namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class WildcardETagValidator : ETagValidator
  {
    public override bool Matches(string entityTag)
    {
      return true;
    }

    public override string ToString()
    {
      return "*";
    }
  }
}