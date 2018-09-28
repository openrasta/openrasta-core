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
using static System.Linq.Expressions.Expression;

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
        model.Hydra().SerializeFunc = CreateDocumentSerializer(model, repository);
      }
    }

    Func<object, SerializationContext, Stream, Task> CreateDocumentSerializer(ResourceModel model,
      IMetaModelRepository repository)
    {
      var renderer = new List<Expression>();
      var variables = new List<ParameterExpression>();

  
      var resourceIn = Parameter(typeof(object), "resource");
      var options = Parameter(typeof(SerializationContext), "options");
      var stream = Parameter(typeof(Stream), "stream");
      var retVal = Variable(typeof(Task), "retVal");

      var resource = Variable(model.ResourceType, "typedResource");
      renderer.Add(Assign(resource, Convert(resourceIn, model.ResourceType)));
      var jsonWriter = Variable(typeof(JsonWriter), "jsonWriter");
      var buffer = Variable(typeof(ArraySegment<byte>), "buffer");


      renderer.Add(Assign(jsonWriter, New(typeof(JsonWriter))));
      
      TypeMethods.ResourceDocument(jsonWriter, model, resource, options, variables.Add, renderer.Add, repository);

      renderer.Add(Assign(buffer, JsonWriterMethods.GetBuffer(jsonWriter)));
      renderer.Add(Assign(retVal, ClassLibMethods.StreamWriteAsync(stream, buffer)));
      renderer.Add(retVal);

      var block = Block(variables.Concat(new[] {jsonWriter, buffer, retVal, resource}).ToArray(), renderer);
      var lambda =
        Lambda<Func<object, SerializationContext, Stream, Task>>(block, "Render", new[] {resourceIn, options, stream});
      return lambda.Compile();
    }
  }
}