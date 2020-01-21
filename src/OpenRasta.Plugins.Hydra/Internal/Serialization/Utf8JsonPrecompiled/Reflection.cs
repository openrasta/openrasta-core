using System.Linq;
using System.Reflection;
// ReSharper disable InconsistentNaming

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  static class Reflection
  {
    public static class ArraySegment<T>
    {
      public static readonly PropertyInfo get_Array =
        typeof(System.ArraySegment<T>).GetProperty(nameof(System.ArraySegment<T>.Array), typeof(T[]));

      public static readonly PropertyInfo get_Offset =
        typeof(System.ArraySegment<T>).GetProperty(nameof(System.ArraySegment<T>.Offset), typeof(int));

      public static readonly PropertyInfo get_Count =
        typeof(System.ArraySegment<T>).GetProperty(nameof(System.ArraySegment<T>.Count), typeof(int));
    }

    public static class Stream
    {
      public static readonly MethodInfo WriteAsyncBytesOffsetCount =
        typeof(System.IO.Stream).GetMethod(nameof(System.IO.Stream.WriteAsync),
          new[] {typeof(byte[]), typeof(int), typeof(int)});
    }

    public static class String
    {
      public static readonly MethodInfo ConcatStringString =
        typeof(string).GetMethod(
          nameof(string.Concat),
          new[] {typeof(string), typeof(string)});
    }

    // ReSharper disable once InconsistentNaming
    public static class HydraJsonFormatterResolver
    {
      public static readonly MethodInfo GetFormatter =
        typeof(Utf8JsonPrecompiled.HydraJsonFormatterResolver).GetMethod(nameof(Utf8Json.IJsonFormatterResolver.GetFormatter));
    }

    public static class Enumerable
    {
      public static readonly MethodInfo ToArray = typeof(System.Linq.Enumerable).GetMethod(nameof(System.Linq.Enumerable.ToArray));

      public static readonly MethodInfo Any =
        typeof(System.Linq.Enumerable).GetMethods().Single(m => m.Name == nameof(System.Linq.Enumerable.Any) && m.GetParameters().Length == 1);
    }
    public static class JsonWriter
    {
      public static readonly MethodInfo WriteBeginObject = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WriteBeginObject));
      public static readonly MethodInfo WriteBeginArray = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WriteBeginArray));
      public static readonly MethodInfo WriteEndArray = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WriteEndArray));
      public static readonly MethodInfo WriteString = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WriteString));
      public static readonly MethodInfo WritePropertyName = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WritePropertyName));
      public static readonly MethodInfo WriteRaw = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WriteRaw), new[] {typeof(byte[])});
      public static readonly MethodInfo WriteValueSeparator = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WriteValueSeparator));
      public static readonly MethodInfo WriteNameSeparator = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WriteNameSeparator));
      public static readonly MethodInfo WriteEndObject = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.WriteEndObject));
      public static readonly MethodInfo GetBuffer = typeof(Utf8Json.JsonWriter).GetMethod(nameof(Utf8Json.JsonWriter.GetBuffer));
    }

    public static class HydraTextExtensions
    {
      public static readonly MethodInfo UriSubResourceCombine = typeof(Serialization.HydraTextExtensions).GetMethod(
        nameof(HydraTextExtensions.UriSubResourceCombine),
        BindingFlags.Static | BindingFlags.NonPublic);
      public static readonly MethodInfo UriStandardCombine = typeof(Serialization.HydraTextExtensions).GetMethod(nameof(Serialization.HydraTextExtensions.UriStandardCombine),
        BindingFlags.Static | BindingFlags.NonPublic);
    }
  }
}