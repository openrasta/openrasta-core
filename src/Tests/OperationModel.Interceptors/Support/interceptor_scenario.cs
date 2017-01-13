using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.TypeSystem;

namespace Tests.OperationModel.Interceptors
{
  public abstract class interceptor_scenario
  {
    protected void given_operation<T>(Expression<Func<T,object>> method) where T:new()
    {
      var visitor = new HandlerMethodVisitor();
      visitor.Visit(method);
      var mi = visitor .Method;
      Operation = new MethodBasedOperationCreator()
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