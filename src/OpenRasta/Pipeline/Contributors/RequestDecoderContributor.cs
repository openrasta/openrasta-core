using System;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Hydrators;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class RequestDecoderContributor : KnownStages.IRequestDecoding
  {
    readonly Func<IRequestEntityReader> _reader;
    
    public RequestDecoderContributor(Func<IRequestEntityReader> reader)
    {
      _reader = reader;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.NotifyAsync(ReadRequestEntityBody).After<KnownStages.ICodecRequestSelection>();
    }

    async Task<PipelineContinuation> ReadRequestEntityBody(ICommunicationContext ctx)
    {
      var operation = await _reader().Read(ctx.PipelineData.OperationsAsync);

      ctx.PipelineData.OperationsAsync = operation.Item1 != RequestReadResult.Success
        ? Enumerable.Empty<IOperationAsync>()
        : new[] {operation.Item2};

      return ctx.PipelineData.OperationsAsync.Any()
        ? PipelineContinuation.Continue
        : ctx.Respond<OperationResult.BadRequest>();
    }
  }
}