using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;
using System.Linq;
using OpenRasta.Collections.Specialized;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Schemas;
using Utf8Json;
using Utf8Json.Resolvers;
using Jil;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8Json
{
  public class Utf8JsonMetaModelHandler : IMetaModelHandler
  {
    readonly IUriResolver _uriResolver;
    bool isRoot = true;
    bool recursed = false;

    public Utf8JsonMetaModelHandler(IUriResolver uriResolver)
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

    public class Event : JsonLd.INode
    {
      public int Id { get; set; }
      public string FirstName { get; set; }
    }

    public Func<object, SerializationContext, Stream, Task> GetHydraFunc(IMetaModelRepository _models)
    {
      return async (entity, options, stream) =>
      {
        if (entity is IEnumerable enumerableEntity)
          entity = ConvertToHydraCollection(enumerableEntity);

        var wrapper = new Wrapper(new Event() {Id = 1, FirstName = "poop"}) {Context = "mycontext", Id = "http://fdd.com"};

        var orig = JSON.Serialize(entity);
        
        var output = JSON.SerializeDynamic(new Wrapper(entity){Context = "ctx"}, new Options(excludeNulls: true, serializationNameFormat: SerializationNameFormat.CamelCase));
        
        JsonSerializer.SetDefaultResolver(StandardResolver.ExcludeNullCamelCase);

//        var output = SpanJson.JsonSerializer.Generic.Utf16
//          .Serialize<Wrapper<Payload>, ExcludeNullsCamelCaseResolver<char>>(wrapper);

        var s1 = JsonSerializer.ToJsonString(wrapper);

        var dic = entity.ToProperties();

        var jsonProperties = Decorate(entity, _models, options.BaseUri, dic);

        var camelCaseProperties = jsonProperties.Where(x => x.Value != null).ToDictionary(key => MakeCamelCase(key.Key), value => value.Value);

//        var model = new UriModel();
//        model.Properties.GetOrAdd<HydraResourceModel>("openrasta.Hydra.ResourceModel");
//        
//        var s = JsonSerializer.ToJsonString(new EntryPoint
//        {
//          Collections = new List<Collection>(new[]
//          {
//            new Collection(typeof(Event),
//              new HydraUriModel(model) {CollectionItemType = typeof(Event), EntryPointUri = "/events/gb", ResourceType = typeof(List<Event>), SearchTemplate = null})
//          })
//        });
//        s = JsonSerializer.PrettyPrint(s);

        //((List<Collection>) camelCaseProperties["collections"])[0].Identifier.Uri.Properties.Clear();
//        ((List<Collection>) camelCaseProperties["collections"])[0].Identifier.Uri.ResourceModel.ResourceType = null;
//        ((List<Collection>) camelCaseProperties["collections"])[0].Identifier.Uri.ResourceModel.ResourceKey = null;
//        ((List<Collection>) camelCaseProperties["collections"])[0].Identifier.Uri.ResourceModel.Properties.Clear();
//        ((List<Collection>) camelCaseProperties["collections"])[0].Identifier.Uri.ResourceModel.Codecs.Clear();
//        ((List<Collection>) camelCaseProperties["collections"])[0].Identifier.Uri.ResourceModel.Handlers.Clear();
//        ((List<Collection>) camelCaseProperties["collections"])[0].Identifier.Uri.ResourceModel.Uris.Clear();

        await JsonSerializer.SerializeAsync(stream, camelCaseProperties);

//        var customConverter = new JsonSerializer
//        {
//          NullValueHandling = NullValueHandling.Ignore,
//          MissingMemberHandling = MissingMemberHandling.Ignore,
//          ContractResolver = new JsonLdContractResolver(options.BaseUri, _uriResolver, _models),
//          TypeNameHandling = TypeNameHandling.None,
//          Converters =
//          {
//            new StringEnumConverter(),
//            new JsonLdTypeRefConverter(_models),
//            new HydraUriModelConverter(options.BaseUri),
//            new ContextDocumentConverter()
//          }
//        };

//        using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true))
//        using (var jsonWriter = new JsonTextWriter(writer))
//        {
//          customConverter.Serialize(jsonWriter, entity);
//        }

        //return Task.CompletedTask;
      };
    }

    public IDictionary<string, object> Decorate(object entity, IMetaModelRepository _models, Uri baseUri, IDictionary<string, object> dic)
    {
      var type = entity.GetType();

      if (entity is Collection collection)
      {
        dic["Member"] = new List<IDictionary<string, object>>();

        foreach (var member in collection.Member)
        {
          recursed = true;
          var data = Decorate(member, _models, baseUri, dic);
          data = data.Where(x => x.Value != null).ToDictionary(key => MakeCamelCase(key.Key), value => value.Value);
          ((List<IDictionary<string, object>>) dic["Member"]).Add(data);
        }

        recursed = false;
      }

      //JsonSerializer.SetDefaultResolver(StandardResolver.AllowPrivateExcludeNullCamelCase);

      //CompositeResolver.RegisterAndSetAsDefault(new[] {new Myforammter(),}, new[] {StandardResolver.AllowPrivateExcludeNullCamelCase});


      var jsonProperties = entity.ToProperties();

      //var list = new List<KeyValuePair<string, object>>(jsonProperties);

      var isNode = entity is JsonLd.INode;

      if (_models.TryGetResourceModel(type, out var resourceModel))
      {
        var hydraModel = resourceModel.Hydra();
        TryAddType(type, jsonProperties, hydraModel);
        if (isNode)
        {
          TryAddId(jsonProperties, entity, baseUri);
        }
      }

      AddContext(jsonProperties, baseUri);

      //UTF8 won't camelCase dictionary keys :(

      //Tried ExpandoObject but same problem

      //dynamic myObj = new ExpandoObject();
      //myObj.@id = "id";
      //((IDictionary<string, object>) myObj)["SpecialAge"] = 123;

      //var x = new Error {Message = "oops"};

      if (entity is Collection hydraCollection)
      {
        jsonProperties["Member"] = dic["Member"];
        var id = jsonProperties["identifier"];
        jsonProperties.Remove("identifier");
        jsonProperties["@id"] = id;
      }


      return jsonProperties;
    }

    public static string MakeCamelCase(string name)
    {
      if (char.IsLower(name[0]))
      {
        return name;
      }

      return string.Concat(char.ToLowerInvariant(name[0]), name.Substring(1));
    }


    void AddContext(IDictionary<string, object> jsonProperties, Uri baseUri)
    {
      if (isRoot && !recursed)
      {
        jsonProperties.Add("@context", _uriResolver.CreateUriFor(typeof(Context), baseUri).ToString());
        isRoot = false;
      }
    }

    void TryAddId(IDictionary<string, object> jsonProperties, object entity, Uri baseUri)
    {
      //TODO Do we need to check if @id property is already present? Skip hydra collection for now
      if (entity is Collection)
      {
        return;
      }

      jsonProperties.Add("@id", _uriResolver.CreateFrom(entity, baseUri));
    }

    void TryAddType(Type type, IDictionary<string, object> jsonProperties, HydraResourceModel model)
    {
      jsonProperties.Add("@type", (model.Vocabulary?.DefaultPrefix == null ? string.Empty : $"{model.Vocabulary.DefaultPrefix}:") + type.Name);
    }

    Collection ConvertToHydraCollection(IEnumerable entity)
    {
      var arrayOfObjects = entity.Cast<object>().ToArray();
      return new Collection
      {
        Member = arrayOfObjects
      };
    }
  }
}