using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class CodeBlock : AnyExpression
  {
    public List<ParameterExpression> Variables { get; } = new List<ParameterExpression>();
    public List<ParameterExpression> Parameters { get; } = new List<ParameterExpression>();
    public List<Expression> Statements { get; } = new List<Expression>();

    public CodeBlock(IEnumerable<AnyExpression> expressions)
    {
      foreach (var expression in expressions.ToArray())
      {
        switch (expression.Inner)
        {
          case ParameterExpression param when expression.Type == ConstructedType.Parameter:
            Parameters.Add(param);
            break;
          case ParameterExpression param when expression.Type == ConstructedType.Var:
            Variables.Add(param);
            break;
          default:
            Statements.Add(expression);
            break;
        }
      }
    }
    public CodeBlock(IEnumerable<Expression> expressions)
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

    public static CodeBlock Convert(Converter legacy)
    {
      var parameters = new List<ParameterExpression>();
      var expressions = new List<Expression>();
      legacy(parameters.Add, expressions.Add);
      return new CodeBlock(parameters.Concat(expressions));
    }

    public override Expression Inner => ToBlock();

    BlockExpression ToBlock()
    {
      return Expression.Block(Variables,Statements);
    }

    public static implicit operator BlockExpression(CodeBlock block) => block.ToBlock();
    public static implicit operator Expression(CodeBlock block) => block.ToBlock();
  }
}