using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using Shouldly;
using Tests.OperationModel.Interceptors.Support;
using Xunit;

namespace Tests.OperationModel.Interceptors.sync_system
{
  public class no_rewriting : interceptor_scenario
  {
    public no_rewriting()
    {
      Resolver = new InternalDependencyResolver()
        .Singleton<IOperationInterceptor, SystemAttribute>()
        .Singleton<IMyService, Service>();
      given_operation<Handler>(h=>h.Get(), Resolver);
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

  public class SystemAttribute : IOperationInterceptor
  {
    public IMyService Service { get; }

    public SystemAttribute(IMyService service)
    {
      Service = service;
    }
      
    public bool Called { get; set; }
    public bool BeforeExecute(IOperation operation)
    {
      Called = true;
      return true;
    }

    public Func<IEnumerable<OutputMember>> RewriteOperation(Func<IEnumerable<OutputMember>> operationBuilder)
    {
      return operationBuilder;
    }

    public bool AfterExecute(IOperation operation, IEnumerable<OutputMember> outputMembers)
    {
      return true;
    }
  }

  public class Service : IMyService{}

  public class Handler
  {
    public object Get()
    {
      return null;
    }
  }

  public interface IMyService
  {
  }
}