using System;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal
{
  public class EntryPointHandler
  {
    readonly IMetaModelRepository _models;
    readonly ICommunicationContext _context;

    public EntryPointHandler(IMetaModelRepository models, ICommunicationContext context)
    {
      _models = models;
      _context = context;
    }

    public EntryPoint Get()
    {
      return new EntryPoint
      {
        Collections = (
          from resource in _models.ResourceRegistrations
          from uri in resource.Uris
          let uriModel = uri.Hydra()
          where uriModel.CollectionItemType != null
          let horridBackwardsIType = TypeSystems.Default.FromClr(uriModel.CollectionItemType)
          let itemModel = _models.ResourceRegistrations.Single(r => Equals(r.ResourceKey, horridBackwardsIType))
          select new Collection()
          {
            Identifier = new Uri(_context.ApplicationBaseUri, new Uri(uriModel.EntryPointUri, UriKind.RelativeOrAbsolute)),
            Search = uriModel.SearchTemplate,
            Manages = {Object = GetTypeName(_models, itemModel)}
          }
        ).ToList()
      };
    }

    static string GetTypeName(IMetaModelRepository models, ResourceModel model)
    {
      var opts = models.CustomRegistrations.OfType<HydraOptions>().Single();
      var hydraResourceModel = model.Hydra();
      return (hydraResourceModel.Vocabulary?.DefaultPrefix == null
               ? string.Empty
               : $"{hydraResourceModel.Vocabulary.DefaultPrefix}:") +
             model.ResourceType.Name;
    }
  }
}