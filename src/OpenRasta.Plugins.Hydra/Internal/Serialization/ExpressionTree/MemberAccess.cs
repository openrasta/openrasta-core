using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class BinaryOperator<T> : TypedExpression<T>
  {
    public BinaryOperator(BinaryExpression inner) : base(inner)
    {
    }
  }
  public class MemberAccess<T> : TypedExpression<T>
  {
    readonly MemberExpression _expression;

    public MemberAccess(MemberExpression expression) : base(expression)
    {
      _expression = expression;
    }

    public static implicit operator MemberExpression(MemberAccess<T> variable)
    {
      return variable._expression;
    }

    public static implicit operator Expression(MemberAccess<T> variable)
    {
      return variable.Inner;
    }
  }
}