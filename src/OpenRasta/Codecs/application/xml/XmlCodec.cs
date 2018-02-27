using System;
using System.Text;
using System.Xml;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  public abstract class XmlCodec : IMediaTypeReader, IMediaTypeWriter
  {
    Action<XmlWriterSettings> _configuration = config => { };

    protected XmlCodec()
    {
      Configuration = delegate { };
    }

    object ICodec.Configuration
    {
      get => Configuration;
      set => Configuration = value as Action<XmlWriterSettings>;
    }

    protected XmlWriter Writer { get; private set; }

    protected Action<XmlWriterSettings> Configuration
    {
      get => _configuration;
      set
      {
        if (value == null) value = config => { };
        _configuration = value;
      }
    }

    public abstract void WriteToCore(object entity, IHttpEntity response);
    public abstract object ReadFrom(IHttpEntity request, IType destinationType, string memberName);

    public virtual void WriteTo(object entity, IHttpEntity response, string[] parameters)
    {
      var xmlSettings = new XmlWriterSettings
      {
        Encoding = new UTF8Encoding(false),
        ConformanceLevel = ConformanceLevel.Document,
        Indent = true,
        NewLineOnAttributes = true,
        OmitXmlDeclaration = false,
        CloseOutput = true,
        CheckCharacters = false
      };
      if (response.Headers.ContentType == null)
        response.Headers.ContentType = new MediaType("application/xml;charset=utf-8");
      else if (response.Headers.ContentType.Matches(MediaType.Xml))
        response.Headers.ContentType.CharSet = "utf-8";
      Configuration(xmlSettings);

      Writer = XmlWriter.Create(response.Stream, xmlSettings);
      WriteToCore(entity, response);
      Writer.Flush();
    }
  }
}