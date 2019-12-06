using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class JsonWriterMethods
  {
    static readonly MethodInfo WriteBeginObjectMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteBeginObject));
    static readonly MethodInfo WriteBeginArrayMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteBeginArray));
    static readonly MethodInfo WriteEndArrayMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteEndArray));
    static readonly MethodInfo WriteStringMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteString));
    static readonly MethodInfo WritePropertyNameMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WritePropertyName));
    static readonly MethodInfo WriteRawMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteRaw), new[] {typeof(byte[])});
    static readonly MethodInfo WriteValueSeparatorMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteValueSeparator));
    static readonly MethodInfo WriteNameSeparatorMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteNameSeparator));
    static readonly MethodInfo WriteEndObjectMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.WriteEndObject));
    static readonly MethodInfo GetBufferMethodInfo = typeof(JsonWriter).GetMethod(nameof(JsonWriter.GetBuffer));

    public static Expression WriteBeginObject(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, WriteBeginObjectMethodInfo);

    public static Expression WriteBeginArray(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, WriteBeginArrayMethodInfo);
    
    public static Expression WriteEndArray(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, WriteEndArrayMethodInfo);

    public static Expression WritePropertyName(this ParameterExpression jsonWriter, string propertyName)
      => Expression.Call(jsonWriter, WritePropertyNameMethodInfo, Expression.Constant(propertyName));

    public static Expression WriteRaw(this ParameterExpression jsonWriter, byte[] rawData)
      => Expression.Call(jsonWriter,
        WriteRawMethodInfo,
        Expression.Constant(rawData));

    public static Expression WriteValueSeparator(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, WriteValueSeparatorMethodInfo);

    public static Expression WriteNameSeparator(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, WriteNameSeparatorMethodInfo);

    public static Expression WriteEndObject(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, WriteEndObjectMethodInfo);

    public static Expression GetBuffer(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, GetBufferMethodInfo);
    
    
    public static Expression WriteBeginObject(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, WriteBeginObjectMethodInfo);

    public static Expression WriteBeginArray(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, WriteBeginArrayMethodInfo);
    
    public static Expression WriteEndArray(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, WriteEndArrayMethodInfo);

    public static Expression WriteString(this Variable<JsonWriter> jsonWriter, string value)
      => Expression.Call(jsonWriter, WriteStringMethodInfo, Expression.Constant(value, typeof(string)));

    public static Expression WriteString(this Variable<JsonWriter> jsonWriter, TypedExpression<string> value)
      => Expression.Call(jsonWriter, WriteStringMethodInfo, value);

    public static Expression WritePropertyName(this Variable<JsonWriter> jsonWriter, Expression value)
      => Expression.Call(jsonWriter, WritePropertyNameMethodInfo, value);

    public static Expression WritePropertyName(this Variable<JsonWriter> jsonWriter, string propertyName)
    {
      var writer = new JsonWriter();
      writer.WritePropertyName(propertyName);
      var bytes = writer.ToUtf8ByteArray();
      return jsonWriter.WriteRaw(bytes);
    }

    public static Expression WriteRaw(this Variable<JsonWriter> jsonWriter, byte[] rawData)
      => Expression.Call(jsonWriter, WriteRawMethodInfo,
        Expression.Constant(rawData));

    public static Expression WriteValueSeparator(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, WriteValueSeparatorMethodInfo);

    public static Expression WriteNameSeparator(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, WriteNameSeparatorMethodInfo);

    public static Expression WriteEndObject(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, WriteEndObjectMethodInfo);

    public static Expression GetBuffer(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, GetBufferMethodInfo);
  }
}