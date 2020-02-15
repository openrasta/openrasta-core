using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Web
{
  class HttpHeaderStore
  {
    readonly Dictionary<string, LinkedList<IEnumerable<string>>> _store =
      new Dictionary<string, LinkedList<IEnumerable<string>>>(StringComparer.OrdinalIgnoreCase);

    public ICollection<string> FieldNames => _store.Keys;

    public void AddFieldValues(string fieldName, params string[] values)
    {
      AddFieldValues(fieldName, (IEnumerable<string>) values);
    }
    public void AddFieldValues(string fieldName, IEnumerable<string> values)
    {
      if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
      
      if (!_store.TryGetValue(fieldName, out var lines))
        _store[fieldName] = lines = new LinkedList<IEnumerable<string>>();
      foreach(var v in values)
        if (v == null)
          throw new ArgumentNullException(nameof(values));
      lines.AddLast(values);
    }

    public string CombineFieldValues(string fieldName)
    {
      if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
      
      return _store.TryGetValue(fieldName, out var vals)
        ? string.Join(",", vals.Select(v => string.Join((string) ",", (IEnumerable<string>) v)))
        : null;
    }

    public void SetFieldValue(string fieldName, string fieldValue)
    {
      if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
      if (fieldValue == null)
      {
        if (_store.ContainsKey(fieldName)) _store.Remove(fieldName);
        return;
      }
      if (!_store.TryGetValue(fieldName, out var lines))
        _store[fieldName] = lines = new LinkedList<IEnumerable<string>>();
      lines.Clear();
      lines.AddFirst(new[] {fieldValue});
    }

    public void RemoveHeaderField(string fieldName)
    {
      if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
      _store.Remove(fieldName);
    }

    public bool ContainsHeaderField(string fieldName)
    {
      if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
      return _store.ContainsKey(fieldName);
    }

    public bool TryGetFieldValues(string fieldName, out LinkedList<IEnumerable<string>> values)
    {
      if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
      values = null;
      return _store.TryGetValue(fieldName, out values);
    }

    public void Clear()
    {
      _store.Clear();
    }
  }
}