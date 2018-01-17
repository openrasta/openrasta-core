using System;
using System.Linq;
using OpenRasta.DI;

namespace OpenRasta.Configuration.MetaModel.Handlers
{
  public class DependencyFactoryHandler : AbstractMetaModelHandler
  {
    public IModelDrivenDependencyRegistration Registrar { get; set; }
    public override void PreProcess(IMetaModelRepository repository)
    {
      var factories = repository.CustomRegistrations.OfType<DependencyFactoryModel>().ToList();
      if (factories.Any() == false) return;
      
      if (Registrar == null)
        throw new NotSupportedException("The container does not support factory injection");
      
      factories.ForEach(model=>Registrar.Register(model));
    }
  }
}