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

    public CodeBlock(IEnumerable<AnyExpression> expressions) : this(expressions.ToArray())
    {
    }

    public CodeBlock(params AnyExpression[] expressions) : base(Expression.Empty())
    {
      foreach (var expression in expressions)
      {
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

    public CodeBlock(IEnumerable<Expression> expressions) : base(Expression.Empty())
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

    public override Expression Inner => ToBlock();

    BlockExpression ToBlock()
    {
      return Expression.Block(Variables, Statements);
    }

    public static implicit operator BlockExpression(CodeBlock block) => block.ToBlock();
    public static implicit operator Expression(CodeBlock block) => block.ToBlock();
  }
}