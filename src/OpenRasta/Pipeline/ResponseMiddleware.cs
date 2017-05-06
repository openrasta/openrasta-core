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
      if (continuation == PipelineContinuation.Abort)
        throw new PipelineAbortedException();
      if (continuation == PipelineContinuation.Continue)
        await Next.Invoke(env);
    }
  }
  public class ResponseMiddleware : AbstractContributorMiddleware
  {
    public ResponseMiddleware(Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor)
      : base(singleTapContributor)
    {
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      var contribState = await Contributor(env);

      if (contribState == PipelineContinuation.Abort)
      {
        env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;
        throw new PipelineAbortedException();
      }
      await Next.Invoke(env);
    }
  }
}
