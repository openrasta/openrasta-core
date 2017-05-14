using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class ResponseMiddleware : AbstractContributorMiddleware
  {
    public ResponseMiddleware(ContributorCall call)
      : base(call)
    {
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      var contribState = await ContributorInvoke(env);

#pragma warning disable 618
      if (contribState == PipelineContinuation.Abort)
      {
        env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;
        throw new PipelineAbortedException();
      }
#pragma warning restore 618

      await Next.Invoke(env);
    }
  }
}
