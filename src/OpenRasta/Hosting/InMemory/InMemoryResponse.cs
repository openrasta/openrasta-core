using System;
using System.IO;
using OpenRasta.Web;

namespace OpenRasta.Hosting.InMemory
{
  public class InMemoryResponse : IResponse
  {
    readonly MemoryStream _outputStream = new MemoryStream();

    public InMemoryResponse()
    {
      Headers = new HttpHeaderDictionary();
      Entity = new HttpEntity(Headers, _outputStream);
    }

    public IHttpEntity Entity { get; set; }

    public HttpHeaderDictionary Headers { get; set; }

    public bool HeadersSent { get; private set; }
    public int StatusCode { get; set; }

    public void WriteHeaders()
    {
      if (HeadersSent)
        throw new InvalidOperationException("HTTP Headers have been sent");

      HeadersSent = true;
    }
  }
}
