using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class Invocation<T> : TypedExpression<T>
  {
    readonly InvocationExpression _expression;

    public Invocation(InvocationExpression expression) : base(expression)
    {
      _expression = expression;
    }

    public static implicit operator InvocationExpression(Invocation<T> variable)
    {
      return variable._expression;
    }

    public static implicit operator Expression(Invocation<T> variable)
    {
      return variable.Inner;
    }
  }
}