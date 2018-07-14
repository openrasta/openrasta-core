using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.TypeSystem;

namespace OpenRasta.Plugins.Hydra.Internal
{
  public class EntryPointHandler
  {
    readonly IMetaModelRepository _models;

    public EntryPointHandler(IMetaModelRepository models)
    {
      _models = models;
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
          select new Collection(uriModel.CollectionItemType, uriModel)
        ).ToList()
      };
    }
  }
}