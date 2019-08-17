using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class TypedExpression<T> : AnyExpression
  {
    public TypedExpression(Expression inner) : base(inner)
    {
    }

    public static implicit operator Expression(TypedExpression<T> variable)
    {
      return variable.Inner;
    }
  }
}