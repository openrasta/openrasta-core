using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs.Newtonsoft.Json
{
  [MediaType("application/json")]
  [MediaType("*/*;q=0.5")]
  [SupportedType(typeof(object))]
  public class NewtonsoftJsonCodec : IMediaTypeReaderAsync, IMediaTypeWriterAsync
  {
    public NewtonsoftJsonCodec(ICommunicationContext context)
    {
      _codecModel = context.PipelineData.ResponseCodec?.CodecModel;
      _context = context;
    }

    NewtonsoftCodecOptions _options = new NewtonsoftCodecOptions();
    readonly CodecModel _codecModel;
    ICommunicationContext _context;

    object ICodec.Configuration
    {
      get => _options;
      set => _options = value as NewtonsoftCodecOptions ?? throw new ArgumentNullException();
    }

    public Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      var jsonSerializer = JsonSerializer.CreateDefault(_options.Settings);
      using (var stringWriter = new StreamWriter(response.Stream, new UTF8Encoding(false), 4096, true))
      using (var jsonTextWriter = new JsonTextWriter(stringWriter))
      {
        jsonTextWriter.Formatting = jsonSerializer.Formatting;
        jsonSerializer.Serialize(jsonTextWriter, entity, null);
      }
      
      if (!_context.Response.HeadersSent && response.Stream.CanSeek)
        response.ContentLength = response.Stream.Position;
      
      return Task.CompletedTask;
    }

    public async Task<object> ReadFrom(IHttpEntity request, IType destinationType, string destinationName)
    {
      var content = await new StreamReader(request.Stream, Encoding.UTF8).ReadToEndAsync();
      return JsonConvert.DeserializeObject(content, destinationType.StaticType, _options.Settings);
    }
  }
}