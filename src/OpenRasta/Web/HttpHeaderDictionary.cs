using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;

namespace OpenRasta.Web
{
  /// <summary>
  /// Provides a list of http headers. In dire need of refactoring to use specific header types similar to http digest.
  /// </summary>
  public class HttpHeaderDictionary : IDictionary<string, string>
  {
    ContentDispositionHeader _contentDisposition;
    long? _contentLength;
    MediaType _contentType;
    readonly HttpHeaderStore _base = new HttpHeaderStore();

    public HttpHeaderDictionary()
    {
    }

    public HttpHeaderDictionary(NameValueCollection sourceDictionary)
    {
      foreach (string key in sourceDictionary.Keys)
       AddValues(key, sourceDictionary.GetValues(key));
    }

    public MediaType ContentType
    {
      get => _contentType;
      set
      {
        _contentType = value;
        _base.SetFieldValue(HttpHeaderNames.ContentType, value == null ? null : value.ToString());
      }
    }

    public long? ContentLength
    {
      get => _contentLength;
      set
      {
        _contentLength = value;

        _base.SetFieldValue(HttpHeaderNames.ContentLength, value == null ? null : value.ToString());
      }
    }

    public ContentDispositionHeader ContentDisposition
    {
      get => _contentDisposition;
      set
      {
        _contentDisposition = value;
        _base.SetFieldValue(HttpHeaderNames.ContentDisposition, value?.ToString());
      }
    }

    public void Add(string key, string value)
    {
      _base.AddFieldValues(key, value);
      UpdateValue(key, value);
    }
    public void AddValues(string key, IEnumerable<string> values)
    {
      _base.AddFieldValues(key, values);
      foreach(var value in values)
        UpdateValue(key, value);
    }

    public bool Remove(string key)
    {
      bool containedHeader = _base.ContainsHeaderField(key);
      if (containedHeader)
        _base.RemoveHeaderField(key);
      UpdateValue(key, null);
      return containedHeader;
    }

    public string this[string key]
    {
      get => _base.ContainsHeaderField(key) ? _base.CombineFieldValues(key) : null;
      set
      {
        _base.SetFieldValue(key, value);
        UpdateValue(key, value);
      }
    }

    public bool ContainsKey(string key)
    {
      return _base.ContainsHeaderField(key);
    }

    public ICollection<string> Keys => _base.FieldNames;

    public bool TryGetValue(string key, out string value)
    {
      value = null;

      if (!_base.ContainsHeaderField(key)) return false;
      value = _base.CombineFieldValues(key);
      return true;
    }

    public ICollection<string> Values => _base.FieldNames.Select(n => _base.CombineFieldValues(n)).ToList();

    public void Add(KeyValuePair<string, string> item)
    {
      Add(item.Key, item.Value);
    }

    public void Clear()
    {
      foreach(var fieldName in _base.FieldNames)
        UpdateValue(fieldName, null);
      _base.Clear();
    }

    public bool Contains(KeyValuePair<string, string> item)
    {
      return _base.TryGetFieldValues(item.Key, out var values)
             && values.Any(v => v.Contains(item.Value));
    }

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
      int position = 0;
      foreach (var kv in this)
        array[arrayIndex + (position++)] = kv;
    }

    public int Count => _base.FieldNames.Count;

    public bool IsReadOnly => false;

    public bool Remove(KeyValuePair<string, string> item)
    {
      if (!_base.TryGetFieldValues(item.Key, out var vals))
        return false;

      LinkedListNode<IEnumerable<string>> node = vals.First;
      do
      {
        if (!node.Value.Contains(item.Value)) continue;

        var newValue = node.Value.Where(v => v != item.Value).ToList();
        if (newValue.Count > 0)
          node.Value = newValue;
        else
          vals.Remove(node);
        
        return true;
      } while ((node = node.Next) != null);

      return false;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      foreach (var key in Keys)
        yield return new KeyValuePair<string, string>(key, _base.CombineFieldValues(key));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    void UpdateValue(string headerName, string value)
    {
      if (headerName.Equals(HttpHeaderNames.ContentType, StringComparison.OrdinalIgnoreCase))
        _contentType = value == null ? null : new MediaType(value);
      else if (headerName.Equals(HttpHeaderNames.ContentLength, StringComparison.OrdinalIgnoreCase))
      {
        if (value != null &&
            long.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var contentLength))
          _contentLength = contentLength;
        else
          _contentLength = null;
      }
      else if (headerName.Equals(HttpHeaderNames.ContentDisposition, StringComparison.OrdinalIgnoreCase))
      {
        _contentDisposition = value == null ? null : new ContentDispositionHeader(value);
      }
    }

    public bool TryGetValues(string headerName, out IEnumerable<string> values)
    {
      if (_base.TryGetFieldValues(headerName, out var valueList))
      {
        values = valueList.SelectMany(_ => _);
        return true;
      }

      values = default;
      return false;
    }
  }
}