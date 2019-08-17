using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class If
  {
    public static ConditionalExpression Then(BinaryOperator<bool> test, Expression ifTrue)
    {
      return Expression.IfThen(test, ifTrue);
    }
    public static ConditionalExpression ThenElse(BinaryOperator<bool> test, Expression ifTrue, Expression ifFalse)
    {
      return Expression.IfThenElse(test, ifTrue, ifFalse);
    }
  }
}