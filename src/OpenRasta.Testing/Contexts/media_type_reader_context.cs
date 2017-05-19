using System;
using System.IO;
using System.Reflection;
using System.Text;
using OpenRasta.Codecs;
using OpenRasta.IO;
using OpenRasta.TypeSystem;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Testing.Contexts
{
  public abstract class media_type_reader_context<TCodec> : codec_context<TCodec>
    where TCodec : ICodec
  {
    object _theResult;

    IRequest Request => Context.Request;

    protected void given_request_content_type(string mediaType)
    {
      Request.Entity.ContentType = new MediaType(mediaType);
    }

    void given_request_stream(string requestData, Encoding encoding)
    {
      given_request_stream(stream =>
      {
        using (var sw = new DeterministicStreamWriter(stream, encoding, StreamActionOnDispose.None))
        {
          sw.Write(requestData);
        }
      });
    }

    protected void given_request_stream(Action<Stream> writer)
    {
      Request.Entity.Stream.Position = 0;

      writer(Request.Entity.Stream);
      Request.Entity.Stream.Position = 0;
    }

    protected void given_request_stream(string requestData)
    {
      given_request_stream(requestData, Encoding.UTF8);
    }


    protected T then_decoding_result<T>()
    {
      _theResult.legacyShouldNotBeNull();
      _theResult.ShouldNotBe(Missing.Value);
      _theResult.ShouldBeAssignableTo<T>();
      return (T) _theResult;
    }

    protected void then_decoding_result_is_missing()
    {
      _theResult.LegacyShouldBe(Missing.Value);
    }

    protected void when_decoding<T>()
    {
      when_decoding<T>("entity");
    }

    protected void when_decoding<T>(string paramName)
    {
      var codecInstance = CreateCodec(Context);
      var codec = codecInstance as IMediaTypeReader;
      if (codec != null)
        _theResult = codec.ReadFrom(Context.Request.Entity, TypeSystems.Default.FromClr(typeof(T)), paramName);
      else
      {
        throw new NullReferenceException();
      }
    }
  }
}