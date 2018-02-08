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

    readonly MemoryStream _tempStream = new MemoryStream();

    public HttpListenerResponse(HttpListenerCommunicationContext context, System.Net.HttpListenerResponse response)
    {
      _context = context;
      _nativeResponse = response;
      Headers = new HttpHeaderDictionary();
      // TODO: Wrap stream and send chunked when needed if write starts before sending response
      Entity = new HttpEntity(Headers, _tempStream);
      _nativeResponse.SendChunked = false;
    }

    public IHttpEntity Entity { get; set; }
    public HttpHeaderDictionary Headers { get; private set; }
    public bool HeadersSent { get; private set; }

    public int StatusCode
    {
      get { return _nativeResponse.StatusCode; }
      set { _nativeResponse.StatusCode = value; }
    }

    public void WriteHeaders()
    {
      if (HeadersSent)
        throw new InvalidOperationException("The headers have already been sent.");
      _nativeResponse.Headers.Clear();
      foreach (var header in Headers.Where(h => h.Key != "Content-Length"))
      {
        try
        {
          _nativeResponse.AddHeader(header.Key, header.Value);
        }
        catch (Exception ex)
        {
          _context?.ServerErrors.Add(new Error {Message = ex.ToString()});
        }
      }

      HeadersSent = true;
      _nativeResponse.ContentLength64 = Headers.ContentLength.GetValueOrDefault();
      // TODO: Enable streaming straight back to native response output sting


      // Guard against a possible HttpListenerException : The specified network name is no longer available
      try
      {
        _tempStream.WriteTo(_nativeResponse.OutputStream);
      }
      catch (HttpListenerException ex)
      {
        _context?.ServerErrors.Add(new Error {Message = ex.ToString()});
      }
    }
  }
}