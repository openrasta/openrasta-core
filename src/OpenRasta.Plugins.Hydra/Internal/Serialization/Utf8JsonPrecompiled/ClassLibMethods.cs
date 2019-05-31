using System;
using System.IO;
using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class ClassLibMethods
  {
    public static Expression ArraySegmentArray(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment,
        typeof(ArraySegment<byte>).GetProperty(nameof(ArraySegment<byte>.Array), typeof(byte[])));
    }

    public static Expression ArraySegmentOffset(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment,
        typeof(ArraySegment<byte>).GetProperty(nameof(ArraySegment<byte>.Offset), typeof(int)));
    }

    public static Expression ArraySegmentCount(Expression arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment,
        typeof(ArraySegment<byte>).GetProperty(nameof(ArraySegment<byte>.Count), typeof(int)));
    }

    public static Expression StreamWriteAsync(Expression stream, Expression arraySegment)
    {
      var writeAsyncMethod =
        typeof(Stream).GetMethod(nameof(Stream.WriteAsync), new[] {typeof(byte[]), typeof(int), typeof(int)});
      return Expression.Call(stream, writeAsyncMethod, ArraySegmentArray(arraySegment), ArraySegmentOffset(arraySegment),
        ArraySegmentCount(arraySegment));
    }
  }
}