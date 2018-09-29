using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet
{
  public class JsonNetMetaModelHandler : IMetaModelHandler
  {
    readonly IUriResolver _uriResolver;

    public JsonNetMetaModelHandler(IUriResolver uriResolver)
    {
      _uriResolver = uriResolver;
    }

    public void PreProcess(IMetaModelRepository repository)
    {
    }

    public void Process(IMetaModelRepository repository)
    {
      foreach (var repositoryResourceRegistration in repository.ResourceRegistrations)
      {
        repositoryResourceRegistration.Hydra().SerializeFunc = GetHydraFunc(repository);
      }
    }

    public Func<object, SerializationContext, Stream, Task> GetHydraFunc(IMetaModelRepository models)
    {
      return (entity, options, stream) =>
      {
        if (entity is IEnumerable enumerableEntity)
          entity = ConvertToHydraCollection(enumerableEntity);

        var customConverter = new JsonSerializer
        {
          NullValueHandling = NullValueHandling.Ignore,
          MissingMemberHandling = MissingMemberHandling.Ignore,
          ContractResolver = new JsonLdContractResolver(options.BaseUri, _uriResolver, models),
          TypeNameHandling = TypeNameHandling.None,
          Converters =
          {
            new StringEnumConverter(),
            new JsonLdTypeRefConverter(models),
            new HydraUriModelConverter(options.BaseUri),
            new ContextDocumentConverter()
          }
        };

        using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true))
        using (var jsonWriter = new JsonTextWriter(writer))
        {
          customConverter.Serialize(jsonWriter, entity);
        }

        return Task.CompletedTask;
      };
    }

    Collection ConvertToHydraCollection(IEnumerable entity)
    {
      var arrayOfObjects = entity.Cast<object>().ToArray();
      var itemType = entity.GetType().GetInterfaces()
        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        .Select(i => i.GetGenericArguments()[0]).Single();
      return new Collection
      {
        Member = arrayOfObjects,
        Manages = { Object = itemType.Name}
      };
    }
  }
}