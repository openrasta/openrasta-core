using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Hydrators;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
  public class RequestDecoderContributor : KnownStages.IRequestDecoding
  {
    Func<IEnumerable<IOperation>, Task<Tuple<RequestReadResult,IOperation>>> DecodeRequest;
    public RequestDecoderContributor(IDependencyResolver resolver)
    {
      if (resolver.HasDependency<IOperationHydrator>())
        DecodeRequest = WrapLegacyHydrator(resolver.Resolve<IOperationHydrator>);
      DecodeRequest = _ => resolver.Resolve<IRequestEntityReader>().Read(_);
    }

    Func<IEnumerable<IOperation>, Task<Tuple<RequestReadResult,IOperation>>> WrapLegacyHydrator(Func<IOperationHydrator> resolve)
    {
      return operations =>
      {
        var op = resolve().Process(operations).FirstOrDefault();
        return op != null
          ? Task.FromResult(Tuple.Create(RequestReadResult.Success, op))
          : Task.FromResult(Tuple.Create<RequestReadResult, IOperation>(RequestReadResult.NoneFound, null));
      };
    }


    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Use(ReadRequestEntityBody).After<KnownStages.ICodecRequestSelection>();
    }

    async Task<PipelineContinuation> ReadRequestEntityBody(ICommunicationContext ctx)
    {
      var operation = await DecodeRequest(ctx.PipelineData.Operations);

      ctx.PipelineData.Operations = operation.Item1 != RequestReadResult.Success
        ? Enumerable.Empty<IOperation>()
        : new[] {operation.Item2};

      return ctx.PipelineData.Operations.Any()
        ? PipelineContinuation.Continue
        : ctx.Respond<OperationResult.BadRequest>();
    }
  }
}
