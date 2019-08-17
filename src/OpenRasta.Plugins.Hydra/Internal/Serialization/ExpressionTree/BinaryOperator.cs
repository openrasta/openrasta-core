using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class BinaryOperator<T> : TypedExpression<T>
  {
    public BinaryOperator(BinaryExpression inner) : base(inner)
    {
    }
  }
}