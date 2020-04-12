using System.Globalization;
using System.Collections.Specialized;
using System.Collections.Generic;
using System;
using System.Linq;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Web
{
  public class UriRegistration
  {
    IList<NameValueCollection> _uriTemplateParameters;
    public ResourceModel ResourceModel { get; }
    public UriModel UriModel { get; }

    public UriRegistration(ResourceModel resourceModel, UriModel uriModel)
    {
      ResourceModel = resourceModel;
      UriModel = uriModel;
    }

    public UriRegistration(string uri, object resourceKey, string uriName = null, CultureInfo ci = null)
    {
      UriModel = new UriModel
      {
          Language = ci,
          Name = uriName,
          Uri = uri ?? throw new ArgumentNullException(nameof(uri)),
        ResourceModel = ResourceModel
      };
      ResourceModel = new ResourceModel
      {
          ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey)),
          Uris = { UriModel }
      };
    }

    [Obsolete("Use the Results property, as this is inaccurate.")]
    public IList<NameValueCollection> UriTemplateParameters => 
        _uriTemplateParameters ??= GenerateLegacyResults();

    List<NameValueCollection> GenerateLegacyResults()
    {
      return Results == null
          ? new List<NameValueCollection>(0)
          : Results.Select(r => new NameValueCollection
          {
              r.Match.QueryStringVariables,
              r.Match.PathSegmentVariables
          }).ToList();
    }

    public object ResourceKey => ResourceModel.ResourceKey;
    public string UriName => UriModel.Name;
    public CultureInfo UriCulture => UriModel.Language;
    public string UriTemplate => UriModel.Uri;
    public IEnumerable<TemplatedUriMatch> Results { get; set; }
  }

  [Obsolete("Please use UriRegistration (2.0 beta 2)", error: true)]
  public class ResourceMatch : UriRegistration
  {
    public ResourceMatch(object resourceKey, string uriName, CultureInfo uriCulture, string uriTemplate)
        : base(uriTemplate, resourceKey, uriName, uriCulture)
    {
    }

    public CultureInfo ResourcePathCulture => UriCulture;
  }
}