using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  public interface IMediaTypeWriterAsync : ICodec
  {
    Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters);
  }
}
