using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class ClassLibMethods
  {
    public static Expression ArraySegmentArray(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment,
        StreamTypes.ArraySegmentGetArrayPropertyInfo);
    }

    public static Expression ArraySegmentOffset(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, StreamTypes.ArraySegmentGetOffsetPropertyInfo);
    }

    public static Expression ArraySegmentCount(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, StreamTypes.ArraySegmentGetCountPropertyInfo);
    }

    public static Expression StreamWriteAsync(Expression stream, Expression arraySegment)
    {
      var writeAsyncMethod = StreamTypes.StreamWriteAsyncBytesOffsetCountMethodInfo;
      return Expression.Call(stream, writeAsyncMethod, ArraySegmentArray(arraySegment), ArraySegmentOffset(arraySegment),
        ArraySegmentCount(arraySegment));
    }
  }
}