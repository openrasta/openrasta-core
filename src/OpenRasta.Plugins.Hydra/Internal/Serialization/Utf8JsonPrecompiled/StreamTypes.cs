using System;
using System.IO;
using System.Linq.Expressions;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  static class StreamTypes
  {
    public static Expression WriteAsync(this Variable<Stream> stream, Variable<ArraySegment<byte>> arraySegment)
    {
      var writeAsyncMethod = Reflection.Stream.WriteAsyncBytesOffsetCount;
      return Expression.Call(stream,
        writeAsyncMethod,
        arraySegment.get_Array(),
        arraySegment.get_Offset(),
        arraySegment.get_Count());
    }

    public static Expression get_Array(this Variable<ArraySegment<byte>> arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, Reflection.ArraySegment<byte>.get_Array);
    }

    public static Expression get_Offset(this Variable<ArraySegment<byte>> arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, Reflection.ArraySegment<byte>.get_Offset);
    }

    public static Expression get_Count(this Variable<ArraySegment<byte>> arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, Reflection.ArraySegment<byte>.get_Count);
    }
  }
}