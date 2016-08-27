using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
  public class RequestDecoderContributor : KnownStages.IRequestDecoding
  {
    Func<IEnumerable<IOperation>, Task<IOperation>> DecodeRequest;
    public RequestDecoderContributor(IDependencyResolver resolver)
    {
      if (resolver.HasDependency<IOperationHydrator>())
        DecodeRequest = _ => Task.FromResult(
          resolver.Resolve<IOperationHydrator>().Process(_).Single());
      DecodeRequest = _ => resolver.Resolve<IRequestEntityReader>().Read(_);
    }


    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Use(ReadRequestEntityBody).After<KnownStages.ICodecRequestSelection>();
    }

    async Task<PipelineContinuation> ReadRequestEntityBody(ICommunicationContext ctx)
    {
      var operation = await DecodeRequest(ctx.PipelineData.Operations);
      ctx.PipelineData.Operations = new[] {operation};
      return ctx.PipelineData.Operations.Any()
        ? PipelineContinuation.Continue
        : ctx.Respond<OperationResult.BadRequest>();
    }
  }

  public static class ContextExtensions
  {
    public static PipelineContinuation Respond(this ICommunicationContext env, OperationResult result)
    {
      env.Response.Entity.Instance = result;
      return PipelineContinuation.RenderNow;
    }

    public static PipelineContinuation Respond<T>(this ICommunicationContext env) where T:OperationResult,new()
    {
      env.Response.Entity.Instance = new T();
      return PipelineContinuation.RenderNow;
    }
  }
}
