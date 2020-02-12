using System;
using System.Collections.Generic;
using System.Linq;
using LibOwin;
using OpenRasta.DI;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Katana
{
  class OwinResponse : IResponse
  {
    int _statusCode;

    bool _headersCommitted;

    readonly Dictionary<string, Action<IOwinResponse, string>> _nativeAssignements;

    public OwinResponse(IOwinContext context)
    {
      _nativeAssignements = new Dictionary<string, Action<IOwinResponse, string>>(StringComparer.OrdinalIgnoreCase)
      {
        ["content-type"] = (r, val) => r.ContentType = val,
        ["content-length"] = (r, val) => r.ContentLength = Headers.ContentLength
      };
      
      var response = context.Response;

      context.Response.OnSendingHeaders(r => r.NativeHeadersSent = true, this);

      NativeContext = response;
      Headers = new HttpHeaderDictionary();
      Entity = new HttpEntity(Headers, context.Response.Body);
      _statusCode = context.Response.StatusCode;
    }

    bool NativeHeadersSent { get; set; }
    IOwinResponse NativeContext { get; }
    public IHttpEntity Entity { get; }
    public HttpHeaderDictionary Headers { get; }

    public bool HeadersSent => NativeHeadersSent || _headersCommitted;

    public int StatusCode
    {
      get => _statusCode;
      set => _statusCode = value;
    }

    void ProtectHeadersSent()
    {
      if (_headersCommitted)
        throw new InvalidOperationException("HTTP Headers have already been sent");
    }

    public void WriteHeaders()
    {
      ProtectHeadersSent();

      NativeContext.StatusCode = _statusCode;
      foreach (var header in Headers)
      {
        if (_nativeAssignements.TryGetValue(header.Key, out var setter))
          setter(NativeContext, header.Value);
        else
          NativeContext.Headers.Set(header.Key, header.Value);
      }

      _headersCommitted = true;
    }
  }
}