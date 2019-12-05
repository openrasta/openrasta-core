using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Schemas;


namespace OpenRasta.Plugins.Hydra.Internal
{
  public class ContextHandler
  {
    readonly HydraOptions _options;
    readonly IEnumerable<(ResourceModel resource, HydraResourceModel hydra)> _defaultClasses;

    public ContextHandler(IMetaModelRepository metaModel)
    {
      _options = metaModel.CustomRegistrations.OfType<HydraOptions>().Single();
      _defaultClasses = (
        from resource in metaModel.ResourceRegistrations
        where resource.ResourceType != null && !resource.Hydra().Collection.IsCollection
        let hydraModel = resource.Hydra()
        where hydraModel.Vocabulary?.Uri == _options.Vocabulary.Uri
        select (resource, hydraModel)).ToList();
    }

    public HydraCore.Context Get()
    {
      return new HydraCore.Context
      {
        DefaultVocabulary = _options.Vocabulary.Uri.ToString(),
        Curies = _options.Curies.ToDictionary(v => v.DefaultPrefix, v => v.Uri),
        Classes = _defaultClasses.ToDictionary(
          c => c.resource.ResourceType.Name,
          c => $"{_options.Vocabulary.Uri}{c.resource.ResourceType.Name}/")
      };
    }
  }
}