using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Internal
{
  public class ApiDocumentationHandler
  {
    readonly IMetaModelRepository _models;

    public ApiDocumentationHandler(IMetaModelRepository models)
    {
      _models = models;
    }

    public ApiDocumentation Get()
    {
      return new ApiDocumentation
      {
        SupportedClasses = GenerateClasses()
      };
    }

    Class[] GenerateClasses()
    {
      return _models.ResourceRegistrations.Select(x => x.Hydra().Class).Where(x => x != null).ToArray();
    }
  }
}