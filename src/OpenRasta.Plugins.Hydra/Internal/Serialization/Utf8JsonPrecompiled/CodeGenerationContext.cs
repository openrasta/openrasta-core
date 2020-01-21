using System;
using System.Linq.Expressions;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class CodeGenerationContext
  {
    public Variable<JsonWriter> JsonWriter { get; }
    public Expression ResourceInstance { get; }
    public Variable<SerializationContext> SerializationContext { get; }
    public Variable<HydraJsonFormatterResolver> JsonFormatterResolver { get; }

    public CodeGenerationContext(
      Variable<JsonWriter> jsonWriter,
      Expression resourceInstance,
      Variable<SerializationContext> serializationContext,
      Variable<HydraJsonFormatterResolver> jsonFormatterResolver)
    {
      JsonWriter = jsonWriter;
      ResourceInstance = resourceInstance;
      SerializationContext = serializationContext;
      JsonFormatterResolver = jsonFormatterResolver;
      
      UriGenerator = SerializationContext.get_UriGenerator();
      TypeGenerator = SerializationContext.get_TypeGenerator();
      BaseUri = SerializationContext.get_BaseUri().ObjectToString();
    }

    public MethodCall<string> BaseUri { get; }

    public MemberAccess<Func<object, string>> TypeGenerator { get; }

    public MemberAccess<Func<object, string>> UriGenerator { get; }


    public CodeGenerationContext Push(Expression resourceInstance)
    {
      return new CodeGenerationContext(JsonWriter,resourceInstance,SerializationContext,JsonFormatterResolver);
    }
  }
}