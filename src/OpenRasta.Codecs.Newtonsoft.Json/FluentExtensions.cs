using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenRasta.Configuration.Fluent;

namespace OpenRasta.Codecs.Newtonsoft.Json
{
  public static class FluentExtensions
  {
    public static ICodecDefinition AsJsonNewtonsoft(
      this ICodecParentDefinition root,
      Action<NewtonsoftCodecOptions> options = null)
    {
      var opt = new NewtonsoftCodecOptions();
      options?.Invoke(opt);
      return root.TranscodedBy<NewtonsoftJsonCodec>(opt);
    }
  }

  public class NewtonsoftCodecOptions
  {
    public JsonSerializerSettings Settings { get; set; } =
      new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters =
        {
          new StringEnumConverter(),
          new UnserializableExceptionConverter()
        }
      };
  }

  public class UnserializableExceptionConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var ex = (Exception) value;
      serializer.Serialize(writer, new {message = ex.Message, content = ex.ToString(), type = ex.GetType().FullName});
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType)
    {
      return typeof(Exception).IsAssignableFrom(objectType)
             && !typeof(ISerializable).IsAssignableFrom(objectType);
    }
  }
}