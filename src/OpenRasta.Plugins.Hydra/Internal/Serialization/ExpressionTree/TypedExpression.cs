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
}