using System.Linq.Expressions;

namespace OpenRasta.Configuration.Fluent.Implementation
{
  public class MethodArgumentVisitor : ExpressionVisitor
  {
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      Expression instance = this.Visit(node.Object);
      Expression[] expressionArray = VisitArguments(node);

      return instance == node.Object && expressionArray == null
        ? (Expression) node
        : Expression.Call(node.Object, node.Method, expressionArray);
    }

    Expression[] VisitArguments(
      IArgumentProvider nodes)
    {
      Expression[] expressionArray = null;
      int index1 = 0;
      for (int argumentCount = nodes.ArgumentCount; index1 < argumentCount; ++index1)
      {
        Expression node = nodes.GetArgument(index1);
        Expression expression = VisitArgument(index1, node);
        if (expressionArray != null)
          expressionArray[index1] = expression;
        else if (expression != node)
        {
          expressionArray = new Expression[argumentCount];
          for (int index2 = 0; index2 < index1; ++index2)
            expressionArray[index2] = nodes.GetArgument(index2);
          expressionArray[index1] = expression;
        }
      }

      return expressionArray;
    }

    protected virtual Expression VisitArgument(int position, Expression arg)
    {
      return Visit(arg);
    }
  }
}