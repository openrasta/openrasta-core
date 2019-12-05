using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas;
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

    public HydraCore.EntryPoint Get()
    {
      return new HydraCore.EntryPoint
      {
        Collections = (
          from resource in _models.ResourceRegistrations
          let resourceHydra =  resource.Hydra()
          where resourceHydra.Collection.IsCollection
          from uri in resource.Uris
          let uriModel = uri.Hydra()
          let horridBackwardsIType = TypeSystems.Default.FromClr(resourceHydra.Collection.ItemType)
          let itemModel = _models.ResourceRegistrations.Single(r => Equals(r.ResourceKey, horridBackwardsIType))
          select HydraCore.Collection.FromModel(_context.ApplicationBaseUri,itemModel, uriModel)
        ).ToList()
      };
    }
  }
}