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
    }

    public IHttpEntity Entity { get; }
    public HttpHeaderDictionary Headers { get; }
    public bool HeadersSent { get; private set; }
    readonly ILogger log = TraceSourceLogger.Instance;

    public int StatusCode
    {
      get => NativeContext.Response.StatusCode;
      set => NativeContext.Response.StatusCode = value;
    }

    HttpContext NativeContext { get; set; }

    public void WriteHeaders()
    {
      if (HeadersSent)
        throw new InvalidOperationException("The headers have already been sent.");
      if (Headers.ContentType != null)
      {
        log.WriteDebug("Writing http header Content-Type:{0}", Headers.ContentType.MediaType);
        NativeContext.Response.AppendHeader("Content-Type", Headers.ContentType.MediaType);
      }
      foreach (var header in Headers.Where(h => h.Key != "Content-Type"))
      {
        try
        {
          log.WriteDebug("Writing http header {0}:{1}", header.Key, header.Value);
          NativeContext.Response.AppendHeader(header.Key, header.Value);
        }
        catch (Exception ex)
        {
          var commcontext = DependencyManager.GetService<ICommunicationContext>();
          commcontext?.ServerErrors.Add(new Error {Message = ex.ToString()});
        }
      }
      HeadersSent = true;
    }
  }
}