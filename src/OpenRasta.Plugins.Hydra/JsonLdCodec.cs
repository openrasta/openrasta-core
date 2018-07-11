using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpenRasta.Codecs;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra
{
  public class JsonLdCodec : IMediaTypeWriterAsync
  {
    static JsonSerializer DefaultSettings = new JsonSerializer
    {
      NullValueHandling = NullValueHandling.Ignore,
      MissingMemberHandling = MissingMemberHandling.Ignore,
      ContractResolver = new CamelCasePropertyNamesContractResolver(),
      Converters =
      {
        new StringEnumConverter()
      }
    };

    IUriResolver uris;

    public JsonLdCodec(IUriResolver uris)
    {
      this.uris = uris;
    }

    public object Configuration { get; set; }
    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      JObject serialized = JObject.FromObject(entity, DefaultSettings);
      serialized["@context"] = uris.CreateUriFor<RootContext>();
      var content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serialized));
      response.ContentLength = content.Length;
      
      await response.Stream.WriteAsync(content, 0, content.Length);
    }
  }
}