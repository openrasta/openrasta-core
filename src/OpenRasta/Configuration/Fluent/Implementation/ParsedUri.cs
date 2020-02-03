using System.Linq.Expressions;

namespace OpenRasta.Configuration.Fluent.Implementation
{
  public class ParsedUri
  {
    public ParsedUri(string uriTemplate, Expression generator)
    {
      UriTemplate = uriTemplate;
      Generator = generator;
    }

    public string UriTemplate { get; set; }
    public Expression Generator { get; set; }
  }
}