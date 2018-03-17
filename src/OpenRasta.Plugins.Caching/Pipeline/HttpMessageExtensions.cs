using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public static class HttpMessageExtensions
  {
    public static bool InvalidConditionalHeaders(this IRequest request,
      Action<IEnumerable<string>> erroneousCombination)
    {
      var conditionals = new[] {"if-modified-since", "if-match", "if-none-match"};
      var present = request.Headers.Keys
        .Where(_ => conditionals.Contains(_, StringComparer.OrdinalIgnoreCase))
        .ToList();
      if (present.Count > 1) erroneousCombination(present);
      return present.Count > 1;
    }

    public static void Header(this IHttpMessage message, string header, Action<string> onPresent)
    {
      string headerValue;
      if (message.Headers.TryGetValue(header, out headerValue))
        onPresent(headerValue);
    }

    public static void HeaderDateTimeOffset(this IHttpMessage message, string header, Action<DateTimeOffset> onParse,
      Action<string> onParseError = null)
    {
      DateTimeOffset dateTimeValue;

      string headerValue;
      if (!message.Headers.TryGetValue(header, out headerValue)) return;
      if (DateTimeOffset.TryParse(headerValue, out dateTimeValue))
        onParse(dateTimeValue);
      else if (onParseError != null)
        onParseError(headerValue);
    }

    public static void AppendList(this IHttpMessage message, string header, string value)
    {
      if (message.Headers.ContainsKey(header) && message.Headers[header].Trim().Length > 0)
        message.Headers[header] += ", " + value;
      else
        message.Headers[header] = value;
    }
  }
}