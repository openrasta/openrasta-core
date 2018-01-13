using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections;

namespace OpenRasta.Pipeline
{
  public partial class PipelineData : DictionaryBase<object, object>
  {
    // Provides compatibility for the existing method signatures, rewire string keys to env

    public new object this[object key]
    {
      get => TryGetValue(key, out var val) ? val : null;
      set => CompatibilityThisSetter(key, value);
    }

    public override void Add(object key, object value)
    {
      if (key is string keyString)
        _env.Add(keyString,value);
      else
        base.Add(key, value);
    }

    public override void Clear()
    {
      base.Clear();
      _env.Clear();
    }

    public override bool ContainsKey(object key)
    {
      return key is string keyString ? _env.ContainsKey(keyString) : base.ContainsKey(key);
    }

    public override int Count => _env.Count + base.Count;
    public override ICollection<object> Keys => _env.Keys.Concat(base.Keys).ToList();
    public override bool Remove(object key)
    {
      return key is string keyString ? _env.Remove(keyString) : base.Remove(key);
    }

    public override bool TryGetValue(object key, out object value)
    {
      return key is string keyString ? _env.TryGetValue(keyString, out value) : base.TryGetValue(key, out value);
    }

    public override ICollection<object> Values => _env.Values.Concat(base.Values).ToList();

    protected override object CompatibilityThisGetter(object key)
    {
      return key is string keyString ? _env[keyString] : base.CompatibilityThisGetter(key);
    }

    protected override void CompatibilityThisSetter(object key, object value)
    {
      if (key is string keyString)
        _env[keyString] = value;
      else
        BaseDictionary[key] = value;
    }
  }
}
