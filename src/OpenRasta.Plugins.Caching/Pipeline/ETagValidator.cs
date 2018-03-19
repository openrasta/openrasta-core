using System.Text.RegularExpressions;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public abstract class ETagValidator
  {
    static readonly Regex _strongEtag = new Regex(string.Format(@"^(?<value>""[{0}]*"")$", BNF.ETAG_C));
    static readonly Regex _weakEtag = new Regex(string.Format(@"^W/(?<value>""[{0}]*"")$", BNF.ETAG_C));

    public static ETagValidator TryParse(string value)
    {
      if (value == "*") return new WildcardETagValidator();
      Match match;

      if ((match = _strongEtag.Match(value)).Success)
        return new StrongETagValidator(match.Groups["value"].Value);
      
      return (match = _weakEtag.Match(value)).Success 
        ? new WeakETagValidator(match.Groups["value"].Value) 
        : null;
    }

    public abstract bool Matches(string entityTag);
  }
}