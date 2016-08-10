using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class BeforeRenderComponent : AbstractPipelineComponent
  {
    public BeforeRenderComponent(
      Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor)
      : base(singleTapContributor)
    {
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      var currentState = env.PipelineData.PipelineStage.CurrentState;

      if (currentState == PipelineContinuation.Continue)
        currentState
          = env.PipelineData.PipelineStage.CurrentState
          = await InvokeSingleTap(env);

      if (currentState == PipelineContinuation.Abort)
        await Abort(env);
      await Next.Invoke(env);
    }
  }
}

