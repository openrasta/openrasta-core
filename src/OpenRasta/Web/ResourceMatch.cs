using System.Globalization;
using System.Collections.Specialized;
using System.Collections.Generic;
using System;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Web
{
  public class UriRegistration
  {
    public ResourceModel ResourceModel { get; }
    public UriModel UriModel { get; }

    public UriRegistration(ResourceModel resourceModel, UriModel uriModel)
      : this(uriModel.Uri, resourceModel.ResourceKey, uriModel.Name, uriModel.Language)
    {
      ResourceModel = resourceModel;
      UriModel = uriModel;
    }

    public UriRegistration(string uriTemplate, object resourceKey, string uriName = null, CultureInfo uriCulture = null)
    {
      UriTemplate = uriTemplate ?? throw new ArgumentNullException(nameof(uriTemplate));
      ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));
      UriTemplateParameters = new List<NameValueCollection>();
      UriName = uriName;
      UriCulture = uriCulture;
    }

    public IList<NameValueCollection> UriTemplateParameters { get; }
    public object ResourceKey { get; }
    public string UriName { get; }
    public CultureInfo UriCulture { get; }
    public string UriTemplate { get; }
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