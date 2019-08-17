using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class TypedExpression<T>
  {
    public TypedExpression(Expression inner)
    {
      Inner = inner;
    }
    public static implicit operator Expression(TypedExpression<T> variable)
    {
      return variable.Inner;
    }

    public Expression Inner { get; }
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

  public class Variable<T> : TypedExpression<T>
  {
    readonly ParameterExpression _var;

    public Variable(ParameterExpression var) : base(var)
    {
      _var = var;
    }

    public static implicit operator ParameterExpression(Variable<T> variable)
    {
      return variable._var;
    }
    public static implicit operator Expression(Variable<T> variable)
    {
      return variable.Inner;
    }
  }
}