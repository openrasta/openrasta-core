using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class StringMethods
  {
    static readonly MethodInfo ConcatTwoParamsMethodInfo =
      typeof(string).GetMethod(nameof(string.Concat), new[] {typeof(string), typeof(string)});

    public static Expression Concat(Expression first, Expression second)
      => Expression.Call(ConcatTwoParamsMethodInfo, first, second);

    public static MethodCall<string> Concat(TypedExpression<string> first,TypedExpression<string> second)
      => new MethodCall<string>(Expression.Call(ConcatTwoParamsMethodInfo, first, second));
  }
}