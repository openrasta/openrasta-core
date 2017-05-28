using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenRasta.Binding;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  [MediaType("multipart/form-data;q=0.5")]
  [SupportedType(typeof(IEnumerable<IMultipartHttpEntity>))]
  [SupportedType(typeof(IDictionary<string, IList<IMultipartHttpEntity>>))]
  public class MultipartFormDataObjectCodec : AbstractMultipartFormDataCodec, IMediaTypeReader
  {
    public MultipartFormDataObjectCodec(ICommunicationContext context, ICodecRepository codecs, ITypeSystem typeSystem,
      IObjectBinderLocator binderLocator, Func<Type, ICodec> codecResolver)
      : base(context, codecs, typeSystem, binderLocator, codecResolver)
    {
    }

    public object ReadFrom(IHttpEntity request, IType destinationType, string parameterName)
    {
      if (destinationType.IsAssignableFrom<IEnumerable<IMultipartHttpEntity>>())
      {
        var multipartReader = new MultipartReader(request.ContentType.Boundary, request.Stream);
        return multipartReader.GetParts();
      }
      if (destinationType.IsAssignableFrom<IDictionary<string, IList<IMultipartHttpEntity>>>())
      {
        return FormData(request);
      }
      var binder = BinderLocator.GetBinder(destinationType);
      if (binder == null)
        throw new InvalidOperationException("Cannot find a binder to create the object");
      binder.Prefixes.Add(parameterName);
      bool wasAnyKeyUsed = ReadKeyValues(request).Aggregate(false, (wasUsed, kv) => kv.SetProperty(binder) || wasUsed);
      var result = binder.BuildObject();

      return wasAnyKeyUsed && result.Successful ? result.Instance : Missing.Value;
    }
  }
}