using System;
using System.IO;
using System.Linq;
using System.Net;
using OpenRasta.Web;

namespace OpenRasta.Hosting.HttpListener
{
  public class HttpListenerResponse : IResponse
  {
    readonly HttpListenerCommunicationContext _context;
    readonly System.Net.HttpListenerResponse _nativeResponse;

    public HttpListenerResponse(HttpListenerCommunicationContext context, System.Net.HttpListenerResponse response)
    {
      _context = context;
      _nativeResponse = response;
      Headers = new HttpHeaderDictionary();
      Entity = new HttpEntity(Headers, response.OutputStream);
    }

    public IHttpEntity Entity { get; set; }
    public HttpHeaderDictionary Headers { get; }
    public bool HeadersSent { get; private set; }

    public int StatusCode
    {
      get => _nativeResponse.StatusCode;
      set => _nativeResponse.StatusCode = value;
    }

    public void WriteHeaders()
    {
      if (HeadersSent) return;

      HeadersSent = true;
      _nativeResponse.Headers.Clear();
      foreach (var header in Headers.Where(h => h.Key != "Content-Length"))
      {
        try
        {
          _nativeResponse.AddHeader(header.Key, header.Value);
        }
        catch (Exception ex)
        {
          _context?.ServerErrors.Add(new Error {Message = ex.Message, Exception = ex});
        }
      }

      if (Headers.ContentLength != null)
        _nativeResponse.ContentLength64 = Headers.ContentLength.Value;
    }
  }
}