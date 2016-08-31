using System.Threading.Tasks;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  public interface IMediaTypeReaderAsync : ICodec
  {
    Task<object> ReadFrom(IHttpEntity request, IType destinationType, string destinationName);
  }
}