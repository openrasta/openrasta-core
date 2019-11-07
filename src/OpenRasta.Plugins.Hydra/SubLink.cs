using System;

namespace OpenRasta.Plugins.Hydra
{
  public class SubLink
  {
    public string Rel { get; }
    public Uri Uri { get; }

    public SubLink(string rel, Uri uri, string type = null)
    {
      Rel = rel;
      Uri = uri;
      Type = type;
    }

    public string Type { get; set; }
  }
}