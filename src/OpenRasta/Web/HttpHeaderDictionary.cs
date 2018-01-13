using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace OpenRasta.Web
{
  /// <summary>
  /// Provides a list of http headers. In dire need of refactoring to use specific header types similar to http digest.
  /// </summary>
  public class HttpHeaderDictionary : IDictionary<string, string>
  {
    readonly IDictionary<string, string> _base = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    ContentDispositionHeader _contentDisposition;
    long? _contentLength;
    MediaType _contentType;
    string HDR_CONTENT_DISPOSITION = "Content-Disposition";
    string HDR_CONTENT_LENGTH = "Content-Length";
    string HDR_CONTENT_TYPE = "Content-Type";

    public HttpHeaderDictionary()
    {
    }

    public HttpHeaderDictionary(NameValueCollection sourceDictionary)
    {
      foreach (string key in sourceDictionary.Keys)
        this[key] = sourceDictionary[key];
    }

    public MediaType ContentType
    {
      get => _contentType;
      set {
        _contentType = value;
        _base[HDR_CONTENT_TYPE] = value == null ? null : value.ToString();
      }
    }

    public long? ContentLength
    {
      get => _contentLength;
      set {
        _contentLength = value;
        _base[HDR_CONTENT_LENGTH] = value == null ? null : value.ToString();
      }
    }

    public ContentDispositionHeader ContentDisposition
    {
      get => _contentDisposition;
      set {
        _contentDisposition = value;
        _base[HDR_CONTENT_DISPOSITION] = value?.ToString();
      }
    }

    public void Add(string key, string value)
    {
      _base.Add(key, value);
      UpdateValue(key, value);
    }

    public bool Remove(string key)
    {
      bool result = _base.Remove(key);
      UpdateValue(key, null);
      return result;
    }

    public string this[string key]
    {
      get => _base.TryGetValue(key, out var result) ? result : null;
      set
      {
        _base[key] = value;
        UpdateValue(key, value);
      }
    }

    public bool ContainsKey(string key)
    {
      return _base.ContainsKey(key);
    }

    public ICollection<string> Keys => _base.Keys;

    public bool TryGetValue(string key, out string value)
    {
      return _base.TryGetValue(key, out value);
    }

    public ICollection<string> Values => _base.Values;

    public void Add(KeyValuePair<string, string> item)
    {
      _base.Add(item.Key, item.Value);
    }

    public void Clear()
    {
      _base.Clear();
    }

    public bool Contains(KeyValuePair<string, string> item)
    {
      return _base.ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
      (_base).CopyTo(array, arrayIndex);
    }

    public int Count => _base.Count;

    public bool IsReadOnly => false;

    public bool Remove(KeyValuePair<string, string> item)
    {
      return Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return _base.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    void UpdateValue(string headerName, string value)
    {
      if (headerName.Equals(HDR_CONTENT_TYPE, StringComparison.OrdinalIgnoreCase))
        _contentType = new MediaType(value);
      else if (headerName.Equals(HDR_CONTENT_LENGTH, StringComparison.OrdinalIgnoreCase))
      {
        if (long.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var contentLength))
          _contentLength = contentLength;
      }
      else if (headerName.Equals(HDR_CONTENT_DISPOSITION, StringComparison.OrdinalIgnoreCase))
      {
        _contentDisposition = new ContentDispositionHeader(value);
      }
    }
  }
}
