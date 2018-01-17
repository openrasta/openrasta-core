using System;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.DI
{
  public interface IRequestScopedResolver
  {
    IDisposable CreateRequestScope();
  }

  public interface IModelDrivenDependencyRegistration
  {
    void Register(DependencyFactoryModel registration);
  }
}