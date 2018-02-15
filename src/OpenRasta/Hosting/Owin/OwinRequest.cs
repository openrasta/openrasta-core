using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using LibOwin;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Katana
{
  class OwinRequest : IRequest
  {
    public OwinRequest(IOwinRequest ctx)
    {
      Uri = ctx.Uri;
      PopulateHeaders(ctx);
      HttpMethod = ctx.Method;
      Entity = new HttpEntity(Headers, ctx.Body);
      CodecParameters = new List<string>();
    }

    public IHttpEntity Entity { get; }
    public HttpHeaderDictionary Headers { get; private set; }
    public Uri Uri { get; set; }
    public string UriName { get; set; }
    public CultureInfo NegotiatedCulture { get; set; }
    public string HttpMethod { get; set; }
    public IList<string> CodecParameters { get; }

    void PopulateHeaders(IOwinRequest ctx)
    {
      var headerCollection = new HttpHeaderDictionary();
      foreach (var header in ctx.Headers)
      {
        headerCollection.Add(header.Key, string.Join(",",header.Value));
      }

      Headers = headerCollection;
    }
  }
}