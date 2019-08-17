using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class PreCompiledUtf8JsonSerializer : IMetaModelHandler
  {
    public void PreProcess(IMetaModelRepository repository)
    {
    }

    public void Process(IMetaModelRepository repository)
    {
      foreach (var model in repository.ResourceRegistrations.Where(r => r.ResourceType != null))
      {
        model.Hydra().SerializeFunc = model.ResourceType == typeof(Context)
          ? CreateContextSerializer()
          : CreateDocumentSerializer(model, repository);
      }
    }

    Func<object, SerializationContext, Stream, Task> CreateContextSerializer()
    {
      // Hack. 3am. meh.
      return (o, context, stream) => JsonSerializer.SerializeAsync(stream, (Context) o, new CustomResolver());
    }

    Func<object, SerializationContext, Stream, Task> CreateDocumentSerializer(ResourceModel model,
      IMetaModelRepository repository)
    {
      var renderer = new List<Expression>();
      var variables = new List<ParameterExpression>();

      var resourceIn = New.Parameter<object>("resource");
      var options = New.Parameter<SerializationContext>("options");
      var stream = New.Parameter<Stream>("stream");
      var retVal = New.Var<Task>("retVal");

      var resource = Expression.Variable(model.ResourceType, "typedResource");

      renderer.Add(Expression.Assign(resource, Expression.Convert(resourceIn, model.ResourceType)));

      var jsonWriter = New.Var<JsonWriter>("jsonWriter");
      var buffer = New.Var<ArraySegment<byte>>("buffer");

      renderer.Add(Expression.Assign(jsonWriter, Expression.New(typeof(JsonWriter))));

      TypeMethods.ResourceDocument(jsonWriter, model, resource, options, variables.Add, renderer.Add, repository);

      renderer.Add(Expression.Assign(buffer, jsonWriter.GetBuffer()));
      renderer.Add(Expression.Assign(retVal, stream.WriteAsync(buffer)));
      renderer.Add(retVal);

      var block = Expression.Block(variables.Concat(new[] {jsonWriter, buffer, retVal, resource}).ToArray(), renderer);
      var lambda =
        Expression.Lambda<Func<object, SerializationContext, Stream, Task>>(block, "Render",
          new ParameterExpression[] {resourceIn, options, stream});
      return lambda.Compile();
    }
  }
}