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
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public abstract class request_entity_reader_context : operation_context<EntityReaderHandler>
  {
    protected IEnumerable<IOperationAsync> Operations { get; set; }

    protected void given_filter()
    {
      RequestEntityReader = new RequestEntityReaderHydrator(Resolver, Request)
      {
        ErrorCollector = Errors,
        Log = new TraceSourceLogger<CodecLogSource>()
      };
    }

    protected void given_operations()
    {
      Operations = new MethodBasedOperationCreator(
        filters: new[] {new TypeExclusionMethodFilter<object>()},
        resolver: Resolver).CreateOperations(new[] {TypeSystem.FromClr<EntityReaderHandler>()}).ToList();
    }

    protected RequestEntityReaderHydrator RequestEntityReader { get; set; }

    protected void given_operation_has_codec_match<TCodec>(string name, MediaType mediaType, float codecScore)
    {
      Operations.First(x=>x.Name == name).SetRequestCodec(new CodecMatch(new CodecRegistration(typeof(TCodec),Guid.NewGuid(),mediaType), codecScore, 1));

    }

    protected void when_filtering_operations()
    {
      try
      {
        ResultOperation = RequestEntityReader.Read(Operations).GetAwaiter().GetResult().Item2;
      }
      catch (Exception e)
      {
        Error = e;
      }
    }

    public Exception Error { get; set; }

    protected void when_entity_is_read()
    {
      when_filtering_operations();
    }

    protected void given_operation_value(string methodName, string parameterName, object parameterValue)
    {
      Operations.First(x => x.Name == methodName)
        .Inputs.Required()
        .First(x => x.Member.Name == parameterName)
        .Binder.SetInstance(parameterValue)
        .ShouldBeTrue();
    }

    protected IOperationAsync ResultOperation { get; private set; }
  }
}
