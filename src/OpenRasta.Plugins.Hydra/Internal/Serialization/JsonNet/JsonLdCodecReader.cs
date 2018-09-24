using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenRasta.Codecs;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet
{
  public class JsonLdCodecReader : IMediaTypeReaderAsync
  {
    public object Configuration { get; set; }

    public async Task<object> ReadFrom(IHttpEntity request, IType destinationType, string destinationName)
    {
      var content = await new StreamReader(request.Stream, Encoding.UTF8).ReadToEndAsync();

      return JsonConvert.DeserializeObject(content, destinationType.StaticType);
    }
  }
}