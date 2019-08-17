using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class Variable<T>
  {
    readonly ParameterExpression _var;

    public Variable(ParameterExpression var)
    {
      _var = var;
    }

    public static implicit operator ParameterExpression(Variable<T> variable)
    {
      return variable._var;
    }
  }
}