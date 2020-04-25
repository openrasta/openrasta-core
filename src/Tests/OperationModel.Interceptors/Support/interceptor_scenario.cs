using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.TypeSystem;

namespace Tests.OperationModel.Interceptors.Support
{
  public abstract class interceptor_scenario
  {
    protected void given_operation<T>(Expression<Func<T, object>> method,
      IDependencyResolver resolver = null) where T : new()
    {
      var mi = HandlerMethodVisitor.FindMethodInfo(method);
      var provider = resolver == null
        ? null
        : new SystemAndAttributesOperationInterceptorProvider(resolver.Resolve<IEnumerable<IOperationInterceptor>>);
      Func<IOperation, IEnumerable<IOperationInterceptor>> syncInterceptorProvider = null;
      if (provider != null)
        syncInterceptorProvider = provider.GetInterceptors;
      var asyncInterceptors = resolver == null
        ? Enumerable.Empty<IOperationInterceptorAsync>()
        : resolver.Resolve<IEnumerable<IOperationInterceptorAsync>>();
      
      Operation = MethodBasedOperationCreator
        .CreateOperationDescriptor(
          TypeSystems.Default.FromClr<T>(),
          TypeSystems.Default.From(mi),
          ()=>asyncInterceptors,
          syncInterceptorProvider, binderLocator: null, resolver: resolver).Create();
    }

    protected IEnumerable<OutputMember> Result { get; set; }
    IOperationAsync Operation { get; set; }

    protected void when_invoking_operation()
    {
      try
      {
        Result = Operation.InvokeAsync().Result;
      }
      catch (Exception e)
      {
        Error = e;
      }
    }

    public Exception Error { get; set; }
  }
}