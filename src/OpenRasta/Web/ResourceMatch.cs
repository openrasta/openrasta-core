using System.Globalization;
using System.Collections.Specialized;
using System.Collections.Generic;
using System;

namespace OpenRasta.Web
{
  public class UriRegistration
  {
    public UriRegistration(string uriTemplate, object resourceKey, string uriName = null, CultureInfo uriCulture = null)
    {
      UriTemplate = uriTemplate ?? throw new ArgumentNullException(nameof(uriTemplate));
      ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));
      UriTemplateParameters = new List<NameValueCollection>();
      UriName = uriName;
      UriCulture = uriCulture;
    }

    public IList<NameValueCollection> UriTemplateParameters { get; private set; }
    public object ResourceKey { get; private set; }
    public string UriName { get; private set; }
    public CultureInfo UriCulture { get; private set; }
    public string UriTemplate { get; private set; }
  }

  [Obsolete("Please use UriRegistration")]
  public class ResourceMatch : UriRegistration
  {
    public ResourceMatch(object resourceKey, string uriName, CultureInfo uriCulture, string uriTemplate)
      : base(uriTemplate, resourceKey, uriName, uriCulture)
    {
    }

    public CultureInfo ResourcePathCulture
    {
      get { return base.UriCulture; }
    }
  }
}