using OpenRasta.Codecs;
using OpenRasta.Web;

namespace Tests.Plugins.Caching
{
  [MediaType("*/*")]
  public class NullCodec : ICodec, IMediaTypeWriter
  {
    public object Configuration { get; set; }

    public void WriteTo(object entity, IHttpEntity response, string[] codecParameters)
    {
    }
  }
}