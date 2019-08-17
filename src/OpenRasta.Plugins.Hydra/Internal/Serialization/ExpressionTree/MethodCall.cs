using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class MethodCall<T> : TypedExpression<T>
  {
    readonly MethodCallExpression _expression;

    public MethodCall(MethodCallExpression expression) : base(expression)
    {
      _expression = expression;
    }

    public static implicit operator MethodCallExpression(MethodCall<T> variable)
    {
      return variable._expression;
    }

    public static implicit operator Expression(MethodCall<T> variable)
    {
      return variable.Inner;
    }
  }
}