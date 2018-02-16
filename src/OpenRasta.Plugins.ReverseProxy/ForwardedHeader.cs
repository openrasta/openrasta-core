using System;
using System.Collections.Generic;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ForwardedHeader
  {
    public static IEnumerable<ForwardedHeader> Parse(string value)
    {
      return new ForwardedHeaderParser(value).Parse();
    }

    public string For
    {
      get => TryGetValue("for", out var ret) ? ret : null;
      set => this["for"] = value;
    }
    public string By
    {
      get => TryGetValue("by", out var ret) ? ret : null;
      set => this["by"] = value;
    }

    public string Host
    {
      get => TryGetValue("host", out var ret) ? ret : null;
      set => this["host"] = value;
    }

    public string Proto
    {
      get => TryGetValue("proto", out var ret) ? ret : null;
      set => this["proto"] = value;
    }
    readonly Dictionary<string, string> _pairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public bool TryGetValue(string key, out string value) => _pairs.TryGetValue(key, out value);
    public string this[string key]
    {
      get => _pairs[key];
      set => _pairs[key] = value;
    }
  }
}