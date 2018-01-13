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
    public OwinResponse(IOwinContext context)
    {
      var response = context.Response;

      NativeContext = response;
      Headers = new HttpHeaderDictionary();
      var delayedStream = new DelayedStream(context.Response.Body);
      Entity = new HttpEntity(Headers, delayedStream);
    }

    IOwinResponse NativeContext { get; }
    public IHttpEntity Entity { get; }
    public HttpHeaderDictionary Headers { get; }
    public bool HeadersSent { get; private set; }

    public int StatusCode
    {
      get => NativeContext.StatusCode;
      set => NativeContext.StatusCode = value;
    }

    public void WriteHeaders()
    {
      if (HeadersSent)
        throw new InvalidOperationException("The headers have already been sent.");
      foreach (var header in Headers.Where(h => h.Key != "Content-Type"))
        try
        {
          NativeContext.Headers.Remove(header.Key);
          NativeContext.Headers.Append(header.Key, header.Value);
        }
        catch (Exception ex)
        {
          var commcontext = DependencyManager.GetService<ICommunicationContext>();
          commcontext?.ServerErrors.Add(new Error {Message = ex.ToString()});
        }

      HeadersSent = true;
      if (Headers.ContentType != null)
        NativeContext.Headers.Add(new KeyValuePair<string, string[]>("Content-Type",
          new[] {Headers.ContentType.MediaType}));

      Entity.Stream.Flush();
    }
  }
}