using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class AspNetRequest : IRequest
  {
    string _httpMethod;

    public AspNetRequest(HttpContext context)
    {
      NativeContext = context;
      Uri = NativeContext.Request.Url;
      Headers = new HttpHeaderDictionary(NativeContext.Request.Headers);

      Entity = new HttpEntity(Headers, NativeContext.Request.InputStream);

      if (!string.IsNullOrEmpty(NativeContext.Request.ContentType))
        Entity.ContentType = new MediaType(NativeContext.Request.ContentType);
      
      CodecParameters = new List<string>();
    }

    public IList<string> CodecParameters { get; }

    public IHttpEntity Entity { get; }

    public HttpHeaderDictionary Headers { get; }

    public string HttpMethod
    {
      get => _httpMethod ?? NativeContext.Request.HttpMethod;
      set => _httpMethod = value;
    }

    public CultureInfo NegotiatedCulture { get; set; }

    public Uri Uri { get; set; }
    public string UriName { get; set; }
    HttpContext NativeContext { get; }
  }
}