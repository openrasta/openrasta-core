using OpenRasta.DI;
using OpenRasta.OperationModel.Interceptors;
using Shouldly;
using Tests.OperationModel.Interceptors.Support;
using Xunit;

namespace Tests.OperationModel.Interceptors.sync_system
{
  public class with_available_dependency : interceptor_scenario
  {
    public with_available_dependency()
    {
      Resolver = new InternalDependencyResolver()
        .Singleton<IOperationInterceptor, SystemAttribute>()
        .Singleton<IMyService, Service>();
      given_operation<Handler>(h => h.Get(), Resolver);
      when_invoking_operation();
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