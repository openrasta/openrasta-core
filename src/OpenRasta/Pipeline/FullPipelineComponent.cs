using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class FullPipelineComponent : IPipelineComponent
  {
    readonly IPipelineComponent _execution;
    readonly IPipelineComponent _rendering;
    readonly IPipelineComponent _catastrophicFail;
    readonly IPipelineComponent _cleanup;

    public FullPipelineComponent(
      IPipelineComponent execution,
      IPipelineComponent rendering,
      IPipelineComponent catastrophicFail,
      IPipelineComponent cleanup)
    {
      _execution = execution;
      _rendering = rendering;
      _catastrophicFail = catastrophicFail;
      _cleanup = cleanup;
    }
    public async Task Invoke(ICommunicationContext env)
    {
      await _execution.Invoke(env);
      await _rendering.Invoke(env);
      if (env.PipelineData.PipelineStage.CurrentState != PipelineContinuation.Finished)
        await _catastrophicFail.Invoke(env);
      await _cleanup.Invoke(env);
    }
  }
}
