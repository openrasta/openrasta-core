using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class JsonWriterMethods
  {
    public static Expression WriteBeginObject(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteBeginObject);

    public static Expression WriteBeginArray(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteBeginArray);
    
    public static Expression WriteEndArray(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteEndArray);

    public static Expression WritePropertyName(this ParameterExpression jsonWriter, string propertyName)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WritePropertyName, Expression.Constant(propertyName));

    public static Expression WriteRaw(this ParameterExpression jsonWriter, byte[] rawData)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteRaw,
        Expression.Constant(rawData));

    public static Expression WriteValueSeparator(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteValueSeparator);

    public static Expression WriteNameSeparator(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteNameSeparator);

    public static Expression WriteEndObject(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteEndObject);

    public static Expression GetBuffer(this ParameterExpression jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.GetBuffer);
    
    
    public static Expression WriteBeginObject(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteBeginObject);

    public static Expression WriteBeginArray(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteBeginArray);
    
    public static Expression WriteEndArray(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteEndArray);

    public static Expression WriteString(this Variable<JsonWriter> jsonWriter, string value)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteString, Expression.Constant(value, typeof(string)));

    public static Expression WriteString(this Variable<JsonWriter> jsonWriter, TypedExpression<string> value)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteString, value);

    public static Expression WritePropertyName(this Variable<JsonWriter> jsonWriter, Expression value)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WritePropertyName, value);

    public static Expression WritePropertyName(this Variable<JsonWriter> jsonWriter, string propertyName)
    {
      var writer = new JsonWriter();
      writer.WritePropertyName(propertyName);
      var bytes = writer.ToUtf8ByteArray();
      return jsonWriter.WriteRaw(bytes);
    }

    public static Expression WriteStringRaw(this Variable<JsonWriter> jsonWriter, string value)
    {
      var writer = new JsonWriter();
      writer.WriteString(value);
      var bytes = writer.ToUtf8ByteArray();
      return jsonWriter.WriteRaw(bytes);
    }
    public static Expression WriteRaw(this Variable<JsonWriter> jsonWriter, byte[] rawData)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteRaw,
        Expression.Constant(rawData));

    public static Expression WriteValueSeparator(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteValueSeparator);

    public static Expression WriteNameSeparator(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteNameSeparator);

    public static Expression WriteEndObject(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.WriteEndObject);

    public static Expression GetBuffer(this Variable<JsonWriter> jsonWriter)
      => Expression.Call(jsonWriter, Reflection.JsonWriter.GetBuffer);
  }
}