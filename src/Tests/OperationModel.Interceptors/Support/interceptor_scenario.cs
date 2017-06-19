using System;
using System.Collections.Generic;
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
      Operation = new MethodBasedOperationCreator(
          resolver: resolver,
          syncInterceptorProvider:
          resolver == null
            ? null
            : new SystemAndAttributesOperationInterceptorProvider(resolver))
        .CreateOperation(TypeSystems.Default.From(mi));
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