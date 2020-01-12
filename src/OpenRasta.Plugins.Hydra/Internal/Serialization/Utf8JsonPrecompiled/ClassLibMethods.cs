using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class ClassLibMethods
  {
    public static Expression ArraySegmentArray(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment,
        Reflection.ArraySegment<byte>.get_Array);
    }

    public static Expression ArraySegmentOffset(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, Reflection.ArraySegment<byte>.get_Offset);
    }

    public static Expression ArraySegmentCount(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, Reflection.ArraySegment<byte>.get_Count);
    }

    public static Expression StreamWriteAsync(Expression stream, Expression arraySegment)
    {
      var writeAsyncMethod = Reflection.Stream.WriteAsyncBytesOffsetCount;
      return Expression.Call(stream, writeAsyncMethod, ArraySegmentArray(arraySegment), ArraySegmentOffset(arraySegment),
        ArraySegmentCount(arraySegment));
    }

    public static MethodCall<string> ObjectToString<T>(this TypedExpression<T> @this) =>
      @this.Call(_ => _.ToString());
  }
}