using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using Shouldly;
using Tests.OperationModel.Interceptors.Support;
using Xunit;

namespace Tests.OperationModel.Interceptors.async_system
{
  public class no_rewriting : interceptor_scenario
  {
    public no_rewriting()
    {
      Resolver = new InternalDependencyResolver()
        .Singleton<IOperationInterceptorAsync, SystemAttribute>()
        .Singleton<IMyService, Service>();
      given_operation<Handler>(h => h.Get(), Resolver);
      when_invoking_operation();
    }

    [Fact]
    public void attribute_is_called()
    {
      var intereptor = Resolver.Resolve<IOperationInterceptorAsync>();
      var attrib = intereptor as SystemAttribute;
      attrib.ShouldNotBeNull();
      attrib.Called.ShouldBeTrue();
      attrib.Service.ShouldBeSameAs(Resolver.Resolve<IMyService>());
    }

    public IDependencyResolver Resolver { get; set; }

    public class Service : IMyService
    {
    }

    public class SystemAttribute : IOperationInterceptorAsync
    {
      public IMyService Service { get; }

      public SystemAttribute(IMyService service)
      {
        Service = service;
      }

      public Func<IOperationAsync, Task<IEnumerable<OutputMember>>> Compose(
        Func<IOperationAsync, Task<IEnumerable<OutputMember>>> next)
      {
        Called = true;
        return next;
      }

      public bool Called { get; set; }
    }

    public class Handler
    {
      public Task Get()
      {
        return null;
      }
    }
  }

  public interface IMyService
  {
  }
}