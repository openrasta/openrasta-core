using System;
using System.Globalization;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Web
{
  public static class TemplatedUriResolverLegacyExtensions
  {
    [Obsolete("Please use the Add method. Removed in 2.0 beta 2.", true)]
    public static void AddUriMapping(this IUriResolver resolver, string uri, object resourceKey, CultureInfo ci, string uriName)
    {
      var uriModel = new UriModel
      {
          Language = ci,
          Name = uriName,
          Uri = uri ?? throw new ArgumentNullException(nameof(uri))
      };
      var resourceModel = new ResourceModel
      {
          ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey)),
          Uris = { uriModel }
      };
      resolver.Add(new UriRegistration(resourceModel, uriModel));
    }
  }
}