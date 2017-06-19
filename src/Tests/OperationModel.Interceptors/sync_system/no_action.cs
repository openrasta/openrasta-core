using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.Pipeline;
using OpenRasta.TypeSystem;
using OpenRasta.Web;
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
        new SystemAndAttributesOperationInterceptorProvider(Resolver));

      Resolver.AddDependency<IMyService, Service>(DependencyLifetime.PerRequest);

      var value = creator
        .CreateOperation(TypeSystems.Default.From(mi))
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

  public class CommContextInterceptor : IOperationInterceptor
  {
    public ICommunicationContext Context { get; }
    public bool Called { get; set; }

    public CommContextInterceptor(ICommunicationContext context)
    {
      Context = context;
    }

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

  public class Service : IMyService
  {
  }

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