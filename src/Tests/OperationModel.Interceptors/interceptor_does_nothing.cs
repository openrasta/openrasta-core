using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.TypeSystem;
using Shouldly;
using Xunit;

namespace Tests.OperationModel.Interceptors
{
  public class interceptor_does_nothing
  {
    public interceptor_does_nothing()
    {
      given_operation<LyingBastardHandler>(handler => handler.Nominal());
      when_invoking_operation();
    }

    [Fact]
    public void operation_was_called_successfully()
    {
      Result.Single().Value.ShouldBe(true);
    }
    public async void when_invoking_operation()
    {
      Result = await Operation.InvokeAsync();
    }

    public IEnumerable<OutputMember> Result { get; set; }

    void given_operation<T>(Expression<Func<T,object>> method) where T:new()
    {
      var mi = new HandlerMethodVisitor().Method;
      Operation = new MethodBasedOperationCreator()
        .CreateOperation(TypeSystems.Default.From(mi));
    }

    public IOperationAsync Operation { get; set; }
  }


  public class HandlerMethodVisitor : ExpressionVisitor
  {
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (this.Method == null)
        Method = node.Method;
      return base.VisitMethodCall(node);
    }

    public MethodInfo Method { get; private set; }
  }

  public class NominalFunctionAttribute : Attribute, IOperationInterceptorAsync
  {
    public Func<IOperationAsync, Task<IEnumerable<OutputMember>>> Invoke(Func<IOperationAsync, Task<IEnumerable<OutputMember>>> next)
    {
      return next;
    }
  }
  public class HullMalfunctionAttribute : Attribute, IOperationInterceptorAsync
  {
    public Func<IOperationAsync, Task<IEnumerable<OutputMember>>> Invoke(Func<IOperationAsync, Task<IEnumerable<OutputMember>>> next)
    {
      return operation => { throw new InvalidOperationException("Seal the hatch now!"); };
    }
  }

  public class LyingBastardHandler
  {
    [NominalFunction]
    public Task<bool> Nominal()
    {
      return Task.FromResult(true);
    }

    /*public Task<bool> HullMalfunction()
    {

    }*/
  }
}