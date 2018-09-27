using System.Linq.Expressions;
using Utf8Json;

namespace Tests.Plugins.Hydra.Utf8Json
{
  public static class JsonWriterMethods
  {
    public static Expression WriteBeginObject(ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteBeginObject)));

    public static Expression WriteString(ParameterExpression jsonWriter, string value)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteString)),
        Expression.Constant(value, typeof(string)));

    public static Expression WriteString(ParameterExpression jsonWriter, Expression value)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteString)), value);

    public static Expression WritePropertyName(ParameterExpression jsonWriter, string propertyName)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WritePropertyName)), Expression.Constant(propertyName));

    public static Expression WriteRaw(ParameterExpression jsonWriter, byte[] rawData)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteRaw), new[] {typeof(byte[])}),
        Expression.Constant(rawData));

    public static Expression WriteValueSeparator(ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteValueSeparator)));

    public static Expression WriteNameSeparator(ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteNameSeparator)));

    public static Expression WriteEndObject(ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteEndObject)));

    public static Expression GetBuffer(ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, typeof(JsonWriter).GetMethod(nameof(JsonWriter.GetBuffer)));
  }
}