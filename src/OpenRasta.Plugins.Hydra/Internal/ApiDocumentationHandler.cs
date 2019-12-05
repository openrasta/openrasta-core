using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas;

namespace OpenRasta.Plugins.Hydra.Internal
{
  public class ApiDocumentationHandler
  {
    readonly IMetaModelRepository _models;

    public ApiDocumentationHandler(IMetaModelRepository models)
    {
      _models = models;
    }

    public HydraCore.ApiDocumentation Get()
    {
      return new HydraCore.ApiDocumentation
      {
        SupportedClasses = GenerateClasses()
      };
    }

    HydraCore.Class[] GenerateClasses()
    {
      return _models.ResourceRegistrations.Select(x => x.Hydra().Class).Where(x => x != null).ToArray();
    }
  }
}