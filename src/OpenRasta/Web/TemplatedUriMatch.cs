using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Web
{
  public class TemplatedUriMatch
  {
    public UriTemplateMatch Match { get; }

    public TemplatedUriMatch(ResourceModel resourceModel, UriModel uriModel, UriTemplateMatch match)
    {
      Match = match;
    }
  }
}