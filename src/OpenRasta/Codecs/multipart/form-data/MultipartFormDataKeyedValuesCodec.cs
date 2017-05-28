using System;
using OpenRasta.Binding;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  [MediaType("multipart/form-data;q=0.5")]
  [SupportedType(typeof(object))]
  public class MultipartFormDataKeyedValuesCodec : AbstractMultipartFormDataCodec,
    IKeyedValuesMediaTypeReader<IMultipartHttpEntity>
  {
    public MultipartFormDataKeyedValuesCodec(ICommunicationContext context, ICodecRepository codecs,
      ITypeSystem typeSystem, IObjectBinderLocator binderLocator, Func<Type, ICodec> codecResolver)
      : base(context, codecs, typeSystem, binderLocator, codecResolver)
    {
    }
  }
}