using System.Linq.Expressions;
using System.Reflection;

namespace Tests.OperationModel.Interceptors
{
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
}