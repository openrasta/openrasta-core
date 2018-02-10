using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Web;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxiedCodec : IMediaTypeWriterAsync
  {
    public object Configuration { get; set; }

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      var content = Encoding.UTF8.GetBytes((string) entity);
      response.ContentLength = content.Length;
      await response.Stream.WriteAsync(content, 0, content.Length);
    }
  }
}