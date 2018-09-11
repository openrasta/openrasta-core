using System;

namespace OpenRasta.Plugins.Hydra
{
  public class Vocabulary
  {
    public Vocabulary(string uri, string defaultPrefix)
      : this(new Uri(uri, UriKind.Absolute), defaultPrefix)
    {
      
    }
    public Vocabulary(Uri uri, string defaultPrefix)
    {
      Uri = uri;
      DefaultPrefix = defaultPrefix;
    }

    public Uri Uri { get; }
    public string DefaultPrefix { get; }

    public static implicit operator Vocabulary(string uri)
    {
      return new Vocabulary(uri, null);
    }
  }
}