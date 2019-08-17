using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class Variable<T> : TypedExpression<T>
  {
    readonly ParameterExpression _var;

    public Variable(ParameterExpression var, bool isParam) : base(var)
    {
      _var = var;
      Type = isParam ? ConstructedType.Parameter : ConstructedType.Var;
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

  public enum ConstructedType
  {
    Var,
    Parameter,
    Any
  }
}