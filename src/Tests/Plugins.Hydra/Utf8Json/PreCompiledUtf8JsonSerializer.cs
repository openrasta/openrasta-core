using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Internal;
using Utf8Json;

namespace Tests.Plugins.Hydra.Utf8Json
{
  public class PreCompiledUtf8JsonSerializer : IMetaModelHandler
  {
    public void PreProcess(IMetaModelRepository repository)
    {
    }

    public void Process(IMetaModelRepository repository)
    {
      foreach (var model in repository.ResourceRegistrations)
      {
        model.Hydra().SerializeFunc = CreateDocumentSerializer(model);
      }
    }

    Func<object, SerializationOptions, Stream, Task> CreateDocumentSerializer(ResourceModel model)
    {
      var renderer = new List<Expression>();
      var variables = new List<ParameterExpression>();

      var stream = Expression.Parameter(typeof(Stream), "stream");
      var resourceIn = Expression.Parameter(typeof(object), "resource");
      var options = Expression.Parameter(typeof(SerializationOptions), "options");


      var resource = Expression.Variable(model.ResourceType, "typedResource");
      renderer.Add(Expression.Assign(resource, Expression.Convert(resourceIn, model.ResourceType)));
      var jsonWriter = Expression.Variable(typeof(JsonWriter), "jsonWriter");
      var buffer = Expression.Variable(typeof(ArraySegment<byte>), "buffer");
      var retVal = Expression.Variable(typeof(Task), "retVal");


      renderer.Add(Expression.Assign(jsonWriter, Expression.New(typeof(JsonWriter))));
      TypeMethods.Resource(jsonWriter, model, resource, variables.Add, renderer.Add);

      renderer.Add(Expression.Assign(buffer, JsonWriterMethods.GetBuffer(jsonWriter)));
      renderer.Add(Expression.Assign(retVal, ClassLibMethods.StreamWriteAsync(stream, buffer)));
      renderer.Add(retVal);

      var block = Expression.Block(variables.Concat(new[] {jsonWriter, buffer, retVal, resource}).ToArray(), renderer);
      var lambda =
        Expression.Lambda<Func<object, SerializationOptions, Stream, Task>>(block, "Render", new[] {resourceIn, options, stream});
      return lambda.Compile();
    }
  }
}