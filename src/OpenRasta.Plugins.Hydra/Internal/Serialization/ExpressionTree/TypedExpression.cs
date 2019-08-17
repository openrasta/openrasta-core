using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public class AnyExpression
  {
    public AnyExpression(Expression inner)
    {
      Inner = inner;
      Type = inner.NodeType == ExpressionType.Parameter ? ConstructedType.Var : ConstructedType.Any;
    }

    public AnyExpression() : this(Expression.Empty())
    {
    }

    public static implicit operator Expression(AnyExpression variable)
    {
      return variable.Inner;
    }

    public static implicit operator AnyExpression(Expression variable)
    {
      return new AnyExpression(variable);
    }

    public virtual Expression Inner { get; }
    public ConstructedType Type { get; protected set; }
  }

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