using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class InlineCode : AnyExpression
  {
    public List<ParameterExpression> Variables { get; } = new List<ParameterExpression>();
    public List<ParameterExpression> Parameters { get; } = new List<ParameterExpression>();
    public List<Expression> Statements { get; } = new List<Expression>();

    public InlineCode(params AnyExpression[] expressions) : base(Expression.Empty())
    {
      
      foreach (var expression in expressions)
      {
        switch (expression)
        {
          case null:
            continue;
          case InlineCode code:
            code.Parameters.ForEach(Parameters.Add);
            code.Variables.ForEach(Variables.Add);
            code.Statements.ForEach(Statements.Add);
            break;
          default:
            switch (expression.Inner)
            {
              case ParameterExpression param1 when expression.Type == ConstructedType.Parameter:
                Parameters.Add(param1);
                break;
              case ParameterExpression param2 when expression.Type == ConstructedType.Var:
                Variables.Add(param2);
                break;
              default:
                Statements.Add(expression);
                break;
            }

            break;
        }
      }
    }
    public InlineCode(IEnumerable<AnyExpression> expressions): this(expressions.ToArray())
    {
    }

    public InlineCode(IEnumerable<Expression> expressions): base(Expression.Empty())
    {
      foreach (var expression in expressions.ToArray())
      {
        switch (expression)
        {
          case ParameterExpression param:
            Variables.Add(param);
            break;
          default:
            Statements.Add(expression);
            break;
        }
      }
    }

    public BlockExpression ToBlock()
    {
      return Expression.Block(Variables, Statements);
    }
    public static InlineCode operator +(InlineCode left, InlineCode right)
    {
      return new InlineCode(left, right);
    }
    public override Expression Inner => throw new InvalidOperationException("Can't do.");
  }
}