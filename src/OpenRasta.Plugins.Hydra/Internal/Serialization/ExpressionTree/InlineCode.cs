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
        if (expression == null)
        {
          continue;
        }
        if (expression is InlineCode code)
        {
          code.Parameters.ForEach(Parameters.Add);
          code.Variables.ForEach(Variables.Add);
          code.Statements.ForEach(Statements.Add);
        }
        else
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
  
    public delegate void Converter(Action<ParameterExpression> param, Action<Expression> statement);

    public static InlineCode Convert(Converter legacy)
    {
      var parameters = new List<ParameterExpression>();
      var expressions = new List<Expression>();
      legacy(parameters.Add, expressions.Add);
      return new InlineCode(parameters.Concat(expressions));
    }

    public void CopyTo(Action<ParameterExpression> defineVar, Action<Expression> addStatement)
    {
      if (Parameters.Any()) throw new InvalidOperationException("Cannot copy blocks containing parameters");
      Variables.ForEach(defineVar);
      Statements.ForEach(addStatement);
    }

    public override Expression Inner => throw new InvalidOperationException("Can't do.");
  }
}