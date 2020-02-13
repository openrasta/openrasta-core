using System;
using System.Linq;
using System.Web;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class AspNetResponse : IResponse
  {
    public AspNetResponse(HttpContext context)
    {
      NativeContext = context;
      Headers = new HttpHeaderDictionary();
      Entity = new HttpEntity(Headers, NativeContext.Response.OutputStream);
//      context.Response.AddOnSendingHeaders(httpContext => _nativeHeadersSent = true);
    }

    public IHttpEntity Entity { get; }
    public HttpHeaderDictionary Headers { get; }
    public bool HeadersSent => _headersSent; //|| _nativeHeadersSent;
    readonly ILogger log = TraceSourceLogger.Instance;

    bool _headersSent;
//    bool _nativeHeadersSent;

    public int StatusCode
    {
      get => NativeContext.Response.StatusCode;
      set => NativeContext.Response.StatusCode = value;
    }

    HttpContext NativeContext { get; set; }

    public void WriteHeaders()
    {
      if (_headersSent) throw new InvalidOperationException("HTTP Headers have been sent");
      
      if (Headers.ContentType != null)
      {
        NativeContext.Response.AppendHeader("Content-Type", Headers.ContentType.MediaType);
      }

      foreach (var header in Headers
        .Where(h => h.Value != null &&
                    !string.Equals(h.Key, "Content-Type", StringComparison.OrdinalIgnoreCase)))
      {
        NativeContext.Response.AppendHeader(header.Key, header.Value);
      }

      _headersSent = true;
    }
  }
}