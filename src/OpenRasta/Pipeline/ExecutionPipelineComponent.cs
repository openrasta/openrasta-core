using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class ExecutionPipelineComponent : AbstractPipelineComponent
  {
    public ExecutionPipelineComponent(
      Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor)
      : base(singleTapContributor)
    {
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      var currentState = env.PipelineData.PipelineStage.CurrentState;

      if (currentState != PipelineContinuation.Continue)
      {
        await Next.Invoke(env);
        return;
      }
      currentState
        = env.PipelineData.PipelineStage.CurrentState
          = await InvokeSingleTap(env);

      if (currentState == PipelineContinuation.Abort)
        await Abort(env);
      await Next.Invoke(env);
    }

    static Task Abort(ICommunicationContext env)
    {
      env.OperationResult = new OperationResult.InternalServerError
      {
        Title = "The request could not be processed because of a fatal error. See log below.",
        ResponseResource = env.ServerErrors
      };
      env.PipelineData.ResponseCodec = null;
      env.Response.Entity.Instance = env.ServerErrors;
      env.Response.Entity.Codec = null;
      env.Response.Entity.ContentLength = null;
      return Task.FromResult(0);
    }
  }
}


