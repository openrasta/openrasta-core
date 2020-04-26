using System;
using System.Collections.Generic;
using System.Globalization;
using OpenRasta.Web;

namespace OpenRasta.Hosting.InMemory
{
  public class InMemoryReadOnlyRequest : IRequest
  {
    IRequest _request;

    public InMemoryReadOnlyRequest(IRequest request)
    {
      _request = request;
      Entity = new HttpEntity(request.Entity.Headers,new ReadOnlyStream(request.Entity.Stream));
    }

    public IHttpEntity Entity { get; }

    public HttpHeaderDictionary Headers => _request.Headers;

    public Uri Uri
    {
      get => _request.Uri;
      set => _request.Uri = value;
    }

    public string UriName
    {
      get => _request.UriName;
      set => _request.UriName = value;
    }

    public CultureInfo NegotiatedCulture
    {
      get => _request.NegotiatedCulture;
      set => _request.NegotiatedCulture = value;
    }

    public string HttpMethod
    {
      get => _request.HttpMethod;
      set => _request.HttpMethod = value;
    }

    public IList<string> CodecParameters => _request.CodecParameters;
  }
}