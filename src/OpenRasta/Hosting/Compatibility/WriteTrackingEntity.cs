using System.Collections.Generic;
using System.IO;
using OpenRasta.Codecs;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Compatibility
{
  public class WriteTrackingEntity : IHttpEntity
  {
    readonly IHttpEntity _entity;

    public WriteTrackingEntity(IHttpEntity entity, WriteTrackingResponse writeTrackingResponse)
    {
      _entity = entity;
      Stream = new WriteTrackingStream(entity.Stream, writeTrackingResponse);
    }


    public void Dispose()
    {
      _entity.Dispose();
    }

    public ICodec Codec
    {
      get => _entity.Codec;
      set => _entity.Codec = value;
    }

    public object Instance
    {
      get => _entity.Instance;
      set => _entity.Instance = value;
    }

    public MediaType ContentType
    {
      get => _entity.ContentType;
      set => _entity.ContentType = value;
    }

    public long? ContentLength
    {
      get => _entity.ContentLength;
      set => _entity.ContentLength = value;
    }

    public HttpHeaderDictionary Headers => _entity.Headers;
    public Stream Stream { get; }

    public IList<Error> Errors => _entity.Errors;
  }
}