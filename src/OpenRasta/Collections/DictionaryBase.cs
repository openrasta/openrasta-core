using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenRasta.Collections
{
  public abstract class DictionaryBase<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
  {
    readonly IDictionary<TKey, TValue> _baseDictionary;

    protected DictionaryBase(IDictionary<TKey, TValue> baseDictionary = null)
    {
      _baseDictionary = baseDictionary ?? new Dictionary<TKey, TValue>();
    }

    protected DictionaryBase(IEqualityComparer<TKey> comparer)
    {
      _baseDictionary = new Dictionary<TKey, TValue>(comparer);
    }

    public int Count => _baseDictionary.Count;

    public bool IsReadOnly => ((IDictionary) _baseDictionary).IsReadOnly;

    public ICollection<TKey> Keys => _baseDictionary.Keys;

    public ICollection<TValue> Values => _baseDictionary.Values;

    bool IDictionary.IsFixedSize => ((IDictionary) _baseDictionary).IsFixedSize;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => _baseDictionary.IsReadOnly;

    bool ICollection.IsSynchronized => ((ICollection) _baseDictionary).IsSynchronized;

    ICollection IDictionary.Keys => ((IDictionary) _baseDictionary).Keys;

    object ICollection.SyncRoot => ((ICollection) _baseDictionary).SyncRoot;

    ICollection IDictionary.Values => ((IDictionary) _baseDictionary).Values;

    public virtual TValue this[TKey key]
    {
      get => _baseDictionary[key];
      set => _baseDictionary[key] = value;
    }

    object IDictionary.this[object key]
    {
      get => this[(TKey) key];
      set => this[(TKey) key] = (TValue) value;
    }

    void ICollection.CopyTo(Array array, int index)
    {
      ((ICollection) _baseDictionary).CopyTo(array, index);
    }

    public virtual void Clear()
    {
      _baseDictionary.Clear();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _baseDictionary.Contains(item);

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      _baseDictionary.CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      if (_baseDictionary.ContainsKey(item.Key) &&
          (ReferenceEquals(item.Value, _baseDictionary[item.Key]) ||
           item.Value.Equals(_baseDictionary[item.Key])))
        return Remove(item.Key);
      return false;
    }

    void IDictionary.Add(object key, object value)
    {
      Add((TKey) key, (TValue) value);
    }

    bool IDictionary.Contains(object key)
    {
      return ((IDictionary) _baseDictionary).Contains(key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return ((IDictionary) _baseDictionary).GetEnumerator();
    }

    void IDictionary.Remove(object key)
    {
      Remove((TKey) key);
    }

    public virtual void Add(TKey key, TValue value)
    {
      _baseDictionary.Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
      return _baseDictionary.ContainsKey(key);
    }

    public virtual bool Remove(TKey key)
    {
      return _baseDictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      return _baseDictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable) _baseDictionary).GetEnumerator();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return _baseDictionary.GetEnumerator();
    }
  }
}