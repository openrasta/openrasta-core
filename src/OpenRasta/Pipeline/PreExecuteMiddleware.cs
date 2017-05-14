using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class PreExecuteMiddleware : AbstractContributorMiddleware
  {
    public PreExecuteMiddleware(ContributorCall call)
      : base(call)
    {
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      var continuation = await ContributorInvoke(env);
      if (continuation != PipelineContinuation.Continue)
        env.PipelineData.PipelineStage.CurrentState = continuation;

#pragma warning disable 618
      if (continuation == PipelineContinuation.Abort)
        throw new PipelineAbortedException();
#pragma warning restore 618

      await Next.Invoke(env);
    }
  }
}