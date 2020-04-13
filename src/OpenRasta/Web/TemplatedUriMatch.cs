using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Web
{
  public class TemplatedUriMatch
  {
    public ResourceModel ResourceModel { get; }
    public UriModel UriModel { get; }
    public UriTemplateMatch Match { get; }

    public TemplatedUriMatch(ResourceModel resourceModel, UriModel uriModel, UriTemplateMatch match)
    {
      ResourceModel = resourceModel;
      UriModel = uriModel;
      Match = match;
    }
  }
}