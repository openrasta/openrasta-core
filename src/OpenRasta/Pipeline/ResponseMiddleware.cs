using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
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
