using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class PreExecuteMiddleware : AbstractContributorMiddleware
  {
    public PreExecuteMiddleware(Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor)
      : base(singleTapContributor)
    {
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      var continuation = await Contributor(env);
      if (continuation != PipelineContinuation.Continue)
        env.PipelineData.PipelineStage.CurrentState = continuation;
      if (continuation == PipelineContinuation.Abort)
        throw new PipelineAbortedException();
      await Next.Invoke(env);
    }
  }
}