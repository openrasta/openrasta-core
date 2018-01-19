using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent.Implementation
{
  public class FluentTarget : IHas, IUses, IFluentTarget
  {
    public FluentTarget(IDependencyResolver resolver, IMetaModelRepository repository)
    {
      Resolver = resolver;
      Repository = repository;
    }

    public IMetaModelRepository Repository { get; }
    public IDependencyResolver Resolver { get; }
    public ITypeSystem TypeSystem => Resolver.Resolve<ITypeSystem>();
  }
  
}