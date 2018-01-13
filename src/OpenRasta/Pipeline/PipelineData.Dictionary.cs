using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Collections;
using OpenRasta.OperationModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public partial class PipelineData : IDictionary<string,object>
  {
    // explicit implementation of IDic<string,object>
    readonly IDictionary<string, object> _env;

    public PipelineData()
      : this(new Dictionary<string,object>())
    {
    }

    public PipelineData(IDictionary<string, object> env)
    {
      _env = env;
      PipelineStage = new PipelineStage();
      Owin = new OwinData(this);
      LegacyKeys = BaseDictionary;
    }
    T SafeGet<T>(string key) where T : class
    {
      return _env.TryGetValue(key, out var o) ? o as T : null;
    }

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
    {
      return _env.GetEnumerator();
    }

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
      _env.Add(item);
    }

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
    {
      return _env.Contains(item);
    }

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
      _env.CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
      return _env.Remove(item);
    }

    void IDictionary<string, object>.Add(string key, object value)
    {
      _env.Add(key, value);
    }

    bool IDictionary<string, object>.ContainsKey(string key)
    {
      return _env.ContainsKey(key);
    }

    bool IDictionary<string, object>.Remove(string key)
    {
      return _env.Remove(key);
    }

    bool IDictionary<string, object>.TryGetValue(string key, out object value)
    {
      return _env.TryGetValue(key, out value);
    }

    object IDictionary<string, object>.this[string key]
    {
      get => _env[key];
      set => _env[key] = value;
    }

    ICollection<string> IDictionary<string, object>.Keys => _env.Keys;
  }

}
