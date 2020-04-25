using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.Pipeline;
using OpenRasta.TypeSystem;
using Shouldly;
using Tests.OperationModel.Interceptors.Support;
using Xunit;

namespace Tests.OperationModel.Interceptors.sync_system
{
  public class with_per_request_dependency : interceptor_scenario
  {
    public with_per_request_dependency()
    {
      Resolver = new InternalDependencyResolver()
        .Singleton<IContextStore, InMemoryContextStore>();

      Resolver.AddDependency<IOperationInterceptor, SystemAttribute>(DependencyLifetime.PerRequest);

      var mi = HandlerMethodVisitor.FindMethodInfo<Handler>(h => h.Get());

      var creator = new MethodBasedOperationCreator(
        resolver: Resolver,
        syncInterceptorProvider:
        new SystemAndAttributesOperationInterceptorProvider(Resolver.Resolve<IEnumerable<IOperationInterceptor>>));

      Resolver.AddDependency<IMyService, Service>(DependencyLifetime.PerRequest);

      var value = creator
        .CreateOperation(TypeSystems.Default.FromClr<Handler>(),TypeSystems.Default.From(mi))
        .InvokeAsync()
        .Result.ToList();
    }

    [Fact]
    public void attribute_is_called()
    {
      var intereptor = Resolver.Resolve<IOperationInterceptor>();
      var attrib = intereptor as SystemAttribute;
      attrib.ShouldNotBeNull();
      attrib.Called.ShouldBeTrue();
      attrib.Service.ShouldBeSameAs(Resolver.Resolve<IMyService>());
    }

    public IDependencyResolver Resolver { get; set; }
  }
}