using System.Collections.Generic;
using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  
  public class New
  {
    public static TypedExpression<T> Const<T>(T @const) => new TypedExpression<T>(Expression.Constant(@const));
    public static Variable<T> Var<T>(string variableName)
    {
      return new Variable<T>(Expression.Variable(typeof(T), variableName));
    }
    public static Variable<T> Var<T>()
    {
      return new Variable<T>(Expression.Variable(typeof(T)));
    }

    public static Variable<T> Parameter<T>(string variableName)
    {
      return new Variable<T>(Expression.Parameter(typeof(T), variableName));
    }

  }

  public class NewCollector
  {
    public List<Expression> DeclaredVariables { get; set; } = new List<Expression>();

    Variable<T> Collect<T>(Variable<T> var)
    {
      DeclaredVariables.Add(var);
      return var;
    }
    public  Variable<T> Var<T>(string variableName)
    {
      return Collect(new Variable<T>(Expression.Variable(typeof(T), variableName)));
    }
    public  Variable<T> Var<T>()
    {
      return Collect(new Variable<T>(Expression.Variable(typeof(T))));
    }

    public Variable<T> Parameter<T>(string variableName)
    {
      return Collect(new Variable<T>(Expression.Parameter(typeof(T), variableName)));
    }
  }
}