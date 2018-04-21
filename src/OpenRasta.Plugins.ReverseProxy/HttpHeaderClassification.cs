using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Plugins.ReverseProxy
{
  public static class HttpHeaderClassification
  {
    static readonly List<string> HopByHopHeader = new List<string>
    {
      "connection",
      "te",
      "transfer-encoding",
      "keep-alive",
      "proxy-authorization",
      "proxy-authentication",
      "trailer",
      "upgrade"
    };

    static readonly List<string> AppendOnForwardHeader = new List<string>
    {
      "server-timing"
    };

    static readonly List<string> ContentHeaders = new List<string>
    {
      "allow",
      "content-disposition",
      "content-encoding",
      "content-language",
      "content-length",
      "content-location",
      "content-md5",
      "content-range",
      "content-type",
      "expires",
      "last-modified"
    };
    
    public static bool IsAppendedOnForwardHeader(string headerKey) =>
      AppendOnForwardHeader.Contains(headerKey, StringComparer.OrdinalIgnoreCase);
    
    public static bool IsHopByHopHeader(string headerKey) =>
      HopByHopHeader.Contains(headerKey, StringComparer.OrdinalIgnoreCase);

    public static bool IsMicrosoftHttpContentHeader(string headerKey) =>
      ContentHeaders.Contains(headerKey, StringComparer.OrdinalIgnoreCase);
  }
}