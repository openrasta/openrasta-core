using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  
  public class New
  {
    public static TypedExpression<T> Const<T>(T @const) => new TypedExpression<T>(Expression.Constant(@const));
    public static Variable<T> Var<T>(string variableName)
    {
      return new Variable<T>(Expression.Variable(typeof(T), variableName), false);
    }
    public static Variable<T> Var<T>()
    {
      return new Variable<T>(Expression.Variable(typeof(T)), false);
    }

    public static Variable<T> Parameter<T>(string variableName)
    {
      return new Variable<T>(Expression.Parameter(typeof(T), variableName), true);
    }

    public static TypedExpression<T> Instance<T>()
    {
      return new TypedExpression<T>(Expression.New(typeof(T)));
    }
  }
}