using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace Tests.Infrastructure
{
  public class NullCodec : IMediaTypeReaderAsync, IMediaTypeWriterAsync
  {
    public object Configuration { get; set; }
    public Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      return Task.CompletedTask;
    }

    public Task<object> ReadFrom(IHttpEntity request, IType destinationType, string destinationName)
    {
      return Task.FromResult(Configuration);
    }
  }
}