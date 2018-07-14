using System;

namespace OpenRasta.Plugins.Hydra
{
  public class Vocabulary
  {
    public Vocabulary(string uri, string defaultPrefix)
    {
      Uri = new Uri(uri, UriKind.Absolute);
      DefaultPrefix = defaultPrefix;
    }

    public Uri Uri { get; }
    public string DefaultPrefix { get; }
  }
}