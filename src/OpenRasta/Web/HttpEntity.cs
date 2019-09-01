using System;
using System.Collections.Generic;
using System.IO;
using OpenRasta.Codecs;

namespace OpenRasta.Web
{
  public class HttpEntity : IHttpEntity
  {
    public HttpEntity(HttpHeaderDictionary messageHeaders, Stream entityBodyStream)
    {
      Headers = messageHeaders;
      Stream = entityBodyStream; 

      Errors = new List<Error>();
    }

    public HttpEntity() : this(new HttpHeaderDictionary(), null)
    {
    }

    [Obsolete("Does nothing [2.6.0]")]
    public string FileName => null;

    public ICodec Codec { get; set; }

    public MediaType ContentType
    {
      get => Headers.ContentType;
      set => Headers.ContentType = value;
    }

    public object Instance { get; set; }

    public long? ContentLength
    {
      get => Headers.ContentLength;
      set => Headers.ContentLength = value;
    }

    public HttpHeaderDictionary Headers { get; }

    public Stream Stream { get; }
    public IList<Error> Errors { get; set; }

    public void Dispose()
    {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
      // kept for compatibility
    }
  }
}