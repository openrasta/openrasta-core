using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;

namespace OpenRasta.Plugins.Hydra.Internal
{
  public class ClassDefinitionHandler : IMetaModelHandler
  {
    public ClassDefinitionHandler()
    {
      
    }

    public void PreProcess(IMetaModelRepository repository)
    {
    }

    public void Process(IMetaModelRepository repository)
    {
      foreach (var resourceRegistration in repository.ResourceRegistrations)
      {
        resourceRegistration.ClassDefinition = new ClassDefinition(resourceRegistration.ResourceType.GetProperties(), resourceRegistration.ResourceType.Name);
      }
    }
  }
}