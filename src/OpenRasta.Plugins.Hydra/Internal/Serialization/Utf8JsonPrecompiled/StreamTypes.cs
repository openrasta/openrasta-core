using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  static class StreamTypes
  {
    public static readonly PropertyInfo ArraySegmentGetArrayPropertyInfo = typeof(ArraySegment<byte>).GetProperty(nameof(ArraySegment<byte>.Array), typeof(byte[]));
    public static readonly PropertyInfo ArraySegmentGetOffsetPropertyInfo = typeof(ArraySegment<byte>).GetProperty(nameof(ArraySegment<byte>.Offset), typeof(int));
    public static readonly PropertyInfo ArraySegmentGetCountPropertyInfo = typeof(ArraySegment<byte>).GetProperty(nameof(ArraySegment<byte>.Count), typeof(int));
    public static readonly MethodInfo StreamWriteAsyncBytesOffsetCountMethodInfo = typeof(Stream).GetMethod(nameof(Stream.WriteAsync), new[] {typeof(byte[]), typeof(int), typeof(int)});

    public static Expression WriteAsync(this Variable<Stream> stream, Variable<ArraySegment<byte>> arraySegment)
    { 
      var writeAsyncMethod =StreamWriteAsyncBytesOffsetCountMethodInfo;
      return Expression.Call(stream, 
        writeAsyncMethod, 
        arraySegment.get_Array(), 
        arraySegment.get_Offset(),
        arraySegment.get_Count());
    }
    public static Expression get_Array(this Variable<ArraySegment<byte>> arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, ArraySegmentGetArrayPropertyInfo);
    }
    public static Expression get_Offset(this Variable<ArraySegment<byte>> arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment,ArraySegmentGetOffsetPropertyInfo);
    }

    public static Expression get_Count(this Variable<ArraySegment<byte>> arraySegment)
    {
      return Expression.MakeMemberAccess(arraySegment, ArraySegmentGetCountPropertyInfo);
    }
  }
}