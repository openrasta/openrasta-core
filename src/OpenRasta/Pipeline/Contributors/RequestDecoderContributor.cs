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
    readonly Func<IEnumerable<IOperationAsync>, Task<Tuple<RequestReadResult,IOperationAsync>>> DecodeRequest;
    public RequestDecoderContributor(IDependencyResolver resolver)
    {
      DecodeRequest = ops => resolver.Resolve<IRequestEntityReader>().Read(ops);
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Use(ReadRequestEntityBody).After<KnownStages.ICodecRequestSelection>();
    }

    async Task<PipelineContinuation> ReadRequestEntityBody(ICommunicationContext ctx)
    {
      var operation = await DecodeRequest(ctx.PipelineData.OperationsAsync);

      ctx.PipelineData.OperationsAsync = operation.Item1 != RequestReadResult.Success
        ? Enumerable.Empty<IOperationAsync>()
        : new[] {operation.Item2};

      return ctx.PipelineData.OperationsAsync.Any()
        ? PipelineContinuation.Continue
        : ctx.Respond<OperationResult.BadRequest>();
    }
  }
}
