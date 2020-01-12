using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class StringMethods
  {
    public static MethodCall<string> Concat(TypedExpression<string> first,TypedExpression<string> second)
      => new MethodCall<string>(Expression.Call(Reflection.String.ConcatStringString, first, second));
  }
}