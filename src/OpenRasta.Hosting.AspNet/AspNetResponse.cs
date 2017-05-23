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

    public long? ContentLength
    {
      get => Headers.ContentLength;
      set => Headers.ContentLength = value;
    }

    public string ContentType
    {
      get => Headers.ContentType.ToString();
      set => Headers.ContentType = new MediaType(value);
    }

    public IHttpEntity Entity { get; set; }
    public HttpHeaderDictionary Headers { get; private set; }
    public bool HeadersSent { get; private set; }
    ILogger log = TraceSourceLogger.Instance;

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
          if (commcontext != null)
            commcontext.ServerErrors.Add(new Error {Message = ex.ToString()});
        }
      }
      HeadersSent = true;
    }
  }
}