using System;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Plugins.Hydra
{
  public static class ModelExtensions
  {
    public static ResourceModel GetResourceModel(this IMetaModelRepository metaModelRepository, Type entityType)
    {
      return TryGetResourceModel(metaModelRepository, entityType, out var model)
        ? model
        : throw new ArgumentException($"Resource {entityType} missing from Configuration.");
    }

    public static bool TryGetResourceModel(this IMetaModelRepository metaModelRepository, Type entityType,
      out ResourceModel model)
    {
      model = metaModelRepository.ResourceRegistrations.FirstOrDefault(r => r.ResourceType == entityType);
      return model != null;
    }
  }
}