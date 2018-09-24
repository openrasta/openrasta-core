using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Schemas;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet
{
  public class JsonLdContractResolver : DefaultContractResolver
  {
    readonly Uri _baseUri;
    readonly IMetaModelRepository _models;
    readonly IUriResolver _uris;
    bool _isRoot = true;

    public JsonLdContractResolver(Uri baseUri, IUriResolver uris, IMetaModelRepository models)
    {
      _baseUri = baseUri;
      _uris = uris;
      _models = models;

      CamelCaseNamingStrategy caseNamingStrategy = new CamelCaseNamingStrategy();
      caseNamingStrategy.ProcessDictionaryKeys = true;
      caseNamingStrategy.OverrideSpecifiedNames = true;
      NamingStrategy = caseNamingStrategy;
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
      var jsonProperties = base.CreateProperties(type, memberSerialization);

      var isNode = typeof(JsonLd.INode).IsAssignableFrom(type);
      
      //var isBlankNode = typeof(JsonLd.IBlankNode).IsAssignableFrom(type);
      
      //if (!isNode && !isBlankNode) return jsonProperties;

      if (_models.TryGetResourceModel(type, out var resourceModel))
      {
        var hydraModel = resourceModel.Hydra();

        TryAddType(type, jsonProperties, hydraModel);
        if (isNode) TryAddId(type, jsonProperties);
      }

      if (_isRoot)
      {
        _isRoot = false;
        AddContext(type, jsonProperties);
      }

      return jsonProperties;
    }


    void AddContext(Type type, IList<JsonProperty> jsonProperties)
    {
      jsonProperties.Insert(index: 0, item: new JsonProperty
      {
        PropertyName = "@context",
        PropertyType = typeof(string),
        DeclaringType = type,
        Readable = true,
        Writable = true,
        ValueProvider = new ConstantValueProvider(value => _uris.CreateUriFor(typeof(Context), _baseUri).ToString())
      });
    }

    static void TryAddType(Type type, IList<JsonProperty> jsonProperties, HydraResourceModel model)
    {
      jsonProperties.Insert(0, new JsonProperty
      {
        PropertyName = "@type",
        PropertyType = typeof(string),
        DeclaringType = type,
        Readable = true,
        Writable = true,
        ValueProvider = new ConstantValueProvider(value =>
          (model.Vocabulary?.DefaultPrefix == null ? string.Empty : $"{model.Vocabulary.DefaultPrefix}:") +
          type.Name)
      });
    }

    void TryAddId(Type type, IList<JsonProperty> jsonProperties)
    {
      var idProperty = jsonProperties.FirstOrDefault(p => p.PropertyName == "@id");
      if (idProperty == null)
      {
        jsonProperties.Insert(0, new JsonProperty
        {
          PropertyName = "@id",
          PropertyType = typeof(Uri),
          DeclaringType = type,
          Readable = true,
          Writable = true,
          ValueProvider = new ConstantValueProvider(value => _uris.CreateFrom(value, _baseUri))
        });
      }
      else
      {
        var idx = jsonProperties.IndexOf(idProperty);
        if (idx != 0)
        {
          jsonProperties.RemoveAt(idx);
          jsonProperties.Insert(index: 0, item: idProperty);
        }
      }
    }

    class ConstantValueProvider : IValueProvider
    {
      readonly Func<object, object> _value;

      public ConstantValueProvider(Func<object, object> value)
      {
        _value = value;
      }

      public void SetValue(object target, object value)
      {
        throw new NotImplementedException();
      }

      public object GetValue(object target)
      {
        return _value(target);
      }
    }
  }
}