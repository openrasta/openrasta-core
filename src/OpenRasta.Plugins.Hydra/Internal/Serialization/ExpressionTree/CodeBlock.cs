using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class CodeBlock
  {
    public List<ParameterExpression> Parameters { get; }=new List<ParameterExpression>();
    public  List<Expression> Statements { get; } = new List<Expression>();

    public CodeBlock(IEnumerable<Expression> expressions)
    {
      foreach (var expression in expressions)
      {
        if (expression is ParameterExpression param)
          Parameters.Add(param);
        else
          Statements.Add(expression);
      }
    }

    public static implicit operator BlockExpression(CodeBlock block)
    {
      return Expression.Block(block.Parameters, block.Statements);
    }
  }
}