using System.Linq.Expressions;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class JsonWriterMethods
  {
    public static Expression WriteBeginObject(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteBeginObject)));

    public static Expression WriteBeginArray(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteBeginArray)));
    
    public static Expression WriteEndArray(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteEndArray)));

    public static Expression WriteString(this ParameterExpression jsonWriter, string value)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteString)),
        Expression.Constant(value, typeof(string)));

    public static Expression WriteString(this ParameterExpression jsonWriter, Expression value)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteString)), value);

    public static Expression WritePropertyName(this ParameterExpression jsonWriter, string propertyName)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WritePropertyName)), Expression.Constant(propertyName));

    public static Expression WriteRaw(this ParameterExpression jsonWriter, byte[] rawData)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteRaw), new[] {typeof(byte[])}),
        Expression.Constant(rawData));

    public static Expression WriteValueSeparator(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteValueSeparator)));

    public static Expression WriteNameSeparator(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteNameSeparator)));

    public static Expression WriteEndObject(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteEndObject)));

    public static Expression GetBuffer(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.GetBuffer)));
  }
}