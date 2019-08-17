using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public static class New
  {
    
    public static Variable<T> Var<T>(string variableName)
    {
      return new Variable<T>(Expression.Variable(typeof(T), variableName));
    }
    public static Variable<T> Parameter<T>(string variableName)
    {
      return new Variable<T>(Expression.Parameter(typeof(T), variableName));
    }
    
  }
}