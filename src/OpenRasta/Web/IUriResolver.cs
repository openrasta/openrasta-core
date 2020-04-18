using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OpenRasta.Web
{
  public interface IUriResolver : ICollection<UriRegistration>
  {
    IDictionary<object, HashSet<string>> UriNames { get; }
    UriRegistration Match(Uri uriToMatch);
    Uri CreateUriFor(Uri baseAddress, object resourceKey, string uriName, NameValueCollection keyValues);
  }

  public interface INewUriResolver
  {
    UriRegistration Match(Uri baseAddress, Uri uriToMatch);
  }
}
