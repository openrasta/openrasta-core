using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Diagnostics.Trace
{
  public class RequestMessageCodec : IMediaTypeWriterAsync
  {
    public object Configuration { get; set; }

    static MediaType MediaType = MediaType.Parse("message/http;version=1.1;msgtype=request").Single();

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      response.ContentType = MediaType;
      var request = (IRequest) entity;
      
      using (var writer = new StreamWriter(response.Stream, Encoding.UTF8, 4096, leaveOpen: true))
      {
        await writer.WriteAsync($"{request.HttpMethod} {request.Uri.AbsolutePath} HTTP/1.1\r\n");
        foreach (var header in request.Headers)
          await writer.WriteAsync($"{header.Key}: {header.Value}\r\n");
        await writer.WriteAsync("\r\n");
        await writer.FlushAsync();
      }
      await request.Entity.Stream.CopyToAsync(response.Stream);
      await request.Entity.Stream.FlushAsync();
    }
  }
}