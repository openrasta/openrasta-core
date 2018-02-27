using System;
using System.Runtime.Serialization;
using OpenRasta.Codecs;
using OpenRasta.TypeSystem;

namespace OpenRasta.Web.Codecs
{
  [MediaType("application/xml;q=0.5", "xml")]
  public class XmlDataContractCodec : XmlCodec
  {
    public override object ReadFrom(IHttpEntity request, IType destinationType, string parameterName)
    {
      if (destinationType.StaticType == null)
        throw new InvalidOperationException();
      return new DataContractSerializer(destinationType.StaticType).ReadObject(request.Stream);
    }

    public override void WriteToCore(object entity, IHttpEntity response)
    {
      new DataContractSerializer(entity.GetType()).WriteObject(Writer, entity);
    }
  }
}