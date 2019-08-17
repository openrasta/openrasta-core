using System.Collections.Generic;
using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
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
      return Collect(new Variable<T>(Expression.Variable(typeof(T), variableName), false));
    }
    public  Variable<T> Var<T>()
    {
      return Collect(new Variable<T>(Expression.Variable(typeof(T)), false));
    }

    public Variable<T> Parameter<T>(string variableName)
    {
      return Collect(new Variable<T>(Expression.Parameter(typeof(T), variableName), true));
    }
  }
}