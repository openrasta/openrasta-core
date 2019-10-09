using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class CompatibilityMarkPipelineAsFinished : AbstractMiddleware
  {
    public override async Task Invoke(ICommunicationContext env)
    {
      try
      {
        await Next.Invoke(env);
      }
      finally
      {
#pragma warning disable 618
        env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Finished;
#pragma warning restore 618
      }
    }
  }
}