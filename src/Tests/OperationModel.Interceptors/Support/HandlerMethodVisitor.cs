using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Tests.OperationModel.Interceptors.Support
{
  public class HandlerMethodVisitor : ExpressionVisitor
  {
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (this.Method == null)
        Method = node.Method;
      return base.VisitMethodCall(node);
    }

    public static MethodInfo FindMethodInfo<T>(Expression<Func<T, object>> expr)
    {
      var visitor = new HandlerMethodVisitor();
      visitor.Visit(expr);
      return visitor.Method;
    }
    public MethodInfo Method { get; private set; }
  }
}