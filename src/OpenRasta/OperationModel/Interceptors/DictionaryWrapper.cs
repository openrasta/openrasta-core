using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.OperationModel.Interceptors
{
  public class DictionaryWrapper : IDictionary<string, object>
  {
    readonly IDictionary _originalDictionary;

    public DictionaryWrapper(IDictionary originalDictionary)
    {
      _originalDictionary = originalDictionary;
    }

    IEnumerator IEnumerable.GetEnumerator() => _originalDictionary.GetEnumerator();

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
      foreach (var key in _originalDictionary.Keys)
      {
        yield return new KeyValuePair<string, object>(key.ToString(), _originalDictionary[key]);
      }
    }

    public void Add(KeyValuePair<string, object> item) => _originalDictionary.Add(item.Key, item.Value);


    public void Clear() => _originalDictionary.Clear();

    public bool Contains(KeyValuePair<string, object> item)
    {
      return _originalDictionary.Contains(item.Key) && _originalDictionary[item.Key] == item.Value;
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
      _originalDictionary.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, object> item)
    {
      return Remove(item.Key);
    }

    public int Count => _originalDictionary.Count;

    public bool ContainsKey(string key) => _originalDictionary.Contains(key);

    public void Add(string key, object value) => _originalDictionary.Add(key, value);

    public bool Remove(string key)
    {
      if (!_originalDictionary.Contains(key)) return false;
      _originalDictionary.Remove(key);
      return true;
    }

    public bool TryGetValue(string key, out object value)
    {
      value = null;
      if (!_originalDictionary.Contains(key)) return false;
      value = _originalDictionary[key];
      return true;
    }

    public object this[string key]
    {
      get { return _originalDictionary[key]; }
      set { _originalDictionary[key] = value; }
    }

    public ICollection<string> Keys => new List<string>(_originalDictionary.Keys.Cast<object>().Select(key=>key.ToString()));

    public ICollection<object> Values => new List<object>(_originalDictionary.Values.Cast<object>());

    public bool IsReadOnly => _originalDictionary.IsReadOnly;
  }
}