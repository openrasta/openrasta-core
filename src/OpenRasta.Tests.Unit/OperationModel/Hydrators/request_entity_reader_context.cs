using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Binding;
using OpenRasta.Codecs;
using OpenRasta.Diagnostics;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Hydrators;
using OpenRasta.OperationModel.Hydrators.Diagnostics;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.TypeSystem;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public abstract class request_entity_reader_context : operation_context<HandlerRequiringInputs>
  {
    protected IEnumerable<IOperationAsync> Operations { get; set; }

    protected void given_entity_reader()
    {
      RequestEntityReader = new RequestEntityReaderHydrator(Resolver, Request)
      {
        ErrorCollector = Errors,
        Log = new TraceSourceLogger<CodecLogSource>()
      };
    }

    protected void given_operations_for<T>()
    {
      Operations = new MethodBasedOperationCreator(
        filters: new[] {new TypeExclusionMethodFilter<object>()},
        resolver: Resolver).CreateOperations(new[] {TypeSystem.FromClr<T>()}).ToList();
    }

    protected RequestEntityReaderHydrator RequestEntityReader { get; set; }

    protected void given_operation_has_codec_match<TCodec>(string name, MediaType mediaType, float codecScore)
    {
      Operations.First(x => x.Name == name)
        .SetRequestCodec(
          new CodecMatch(new CodecRegistration(typeof(TCodec), Guid.NewGuid(), mediaType), codecScore, 1));
    }

    protected void when_filtering_operations()
    {
      try
      {
        var result = RequestEntityReader.Read(Operations).GetAwaiter().GetResult();
        ResultOperation = result.Item2;
        ReadResult = result.Item1;
      }
      catch (Exception e)
      {
        Error = e;
      }
    }
    
    public RequestReadResult ReadResult { get; set; }

    public Exception Error { get; set; }

    protected void given_operation_value(string methodName, string parameterName, object parameterValue)
    {
      Operations.First(x => x.Name == methodName)
        .Inputs.Required()
        .First(x => x.Member.Name == parameterName)
        .Binder.SetInstance(parameterValue).ShouldBeTrue();
    }

    protected IOperationAsync ResultOperation { get; private set; }
  }
}