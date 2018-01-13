using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenRasta.Collections
{
  public abstract class DictionaryBase<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
  {
    protected readonly IDictionary<TKey, TValue> BaseDictionary;

    protected DictionaryBase(IDictionary<TKey, TValue> baseDictionary = null)
    {
      BaseDictionary = baseDictionary ?? new Dictionary<TKey, TValue>();
    }

    protected DictionaryBase(IEqualityComparer<TKey> comparer)
    {
      BaseDictionary = new Dictionary<TKey, TValue>(comparer);
    }


    bool IDictionary.IsFixedSize => ((IDictionary) BaseDictionary).IsFixedSize;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => BaseDictionary.IsReadOnly;

    bool ICollection.IsSynchronized => ((ICollection) BaseDictionary).IsSynchronized;

    ICollection IDictionary.Keys => ((IDictionary) BaseDictionary).Keys;

    object ICollection.SyncRoot => ((ICollection) BaseDictionary).SyncRoot;

    ICollection IDictionary.Values => ((IDictionary) BaseDictionary).Values;

    object IDictionary.this[object key]
    {
      get => this[(TKey) key];
      set => this[(TKey) key] = (TValue) value;
    }

    void ICollection.CopyTo(Array array, int index)
    {
      ((ICollection) BaseDictionary).CopyTo(array, index);
    }


    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => BaseDictionary.Contains(item);

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      BaseDictionary.CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      if (BaseDictionary.ContainsKey(item.Key) &&
          (ReferenceEquals(item.Value, BaseDictionary[item.Key]) ||
           item.Value.Equals(BaseDictionary[item.Key])))
        return Remove(item.Key);
      return false;
    }

    void IDictionary.Add(object key, object value)
    {
      Add((TKey) key, (TValue) value);
    }

    bool IDictionary.Contains(object key)
    {
      return ((IDictionary) BaseDictionary).Contains(key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return ((IDictionary) BaseDictionary).GetEnumerator();
    }

    void IDictionary.Remove(object key)
    {
      Remove((TKey) key);
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable) BaseDictionary).GetEnumerator();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return BaseDictionary.GetEnumerator();
    }
    
    
    public virtual int Count => BaseDictionary.Count;

    public bool IsReadOnly => ((IDictionary) BaseDictionary).IsReadOnly;

    public virtual ICollection<TKey> Keys => BaseDictionary.Keys;

    public virtual ICollection<TValue> Values => BaseDictionary.Values;
    
    public virtual void Add(TKey key, TValue value)
    {
      BaseDictionary.Add(key, value);
    }

    public virtual bool ContainsKey(TKey key)
    {
      return BaseDictionary.ContainsKey(key);
    }

    public virtual bool Remove(TKey key)
    {
      return BaseDictionary.Remove(key);
    }

    public virtual bool TryGetValue(TKey key, out TValue value)
    {
      return BaseDictionary.TryGetValue(key, out value);
    }
    
    public virtual void Clear()
    {
      BaseDictionary.Clear();
    }
    
    public virtual TValue this[TKey key]
    {
      get => CompatibilityThisGetter(key);
      set => CompatibilityThisSetter(key, value);
    }

    protected virtual void CompatibilityThisSetter(TKey key, TValue value)
    {
      BaseDictionary[key] = value;
    }

    protected virtual TValue CompatibilityThisGetter(TKey key)
    {
      return BaseDictionary[key];
    }
  }
}