using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OpenRasta.Web
{
  public class BaseAddressInjectingUriResolver : IUriResolver
  {
    readonly Func<ICommunicationContext> _ctx;
    readonly IUriResolver _inner;

    public BaseAddressInjectingUriResolver(Func<ICommunicationContext> ctx)
    {
      _ctx = ctx;
      _inner = new TemplatedUriResolver();
    }

    public IEnumerator<UriRegistration> GetEnumerator()
    {
      return _inner.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable) _inner).GetEnumerator();
    }

    public void Add(UriRegistration item)
    {
      _inner.Add(item);
    }

    public void Clear()
    {
      _inner.Clear();
    }

    public bool Contains(UriRegistration item)
    {
      return _inner.Contains(item);
    }

    public void CopyTo(UriRegistration[] array, int arrayIndex)
    {
      _inner.CopyTo(array, arrayIndex);
    }

    public bool Remove(UriRegistration item)
    {
      return _inner.Remove(item);
    }

    public int Count => _inner.Count;

    public bool IsReadOnly => _inner.IsReadOnly;

    public IDictionary<object, HashSet<string>> UriNames => _inner.UriNames;

    public UriRegistration Match(Uri uriToMatch)
    {
      return _inner.Match(uriToMatch);
    }

    public Uri CreateUriFor(Uri baseAddress, object resourceKey, string uriName, NameValueCollection keyValues)
    {
      return _inner.CreateUriFor(baseAddress ?? _ctx().ApplicationBaseUri, resourceKey, uriName, keyValues);
    }
  }
}