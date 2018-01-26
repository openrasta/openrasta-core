using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.DI
{
  public interface IModelDrivenDependencyRegistration
  {
    void Register(DependencyFactoryModel registration);
  }
}