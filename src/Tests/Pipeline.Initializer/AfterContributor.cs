using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace Tests.Pipeline.Initializer
{
  public class AfterContributor<T> : IPipelineContributor where T : IPipelineContributor
  {
    PipelineContinuation DoNothing(ICommunicationContext c)
    {
      return PipelineContinuation.Continue;
    }

    public virtual void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(DoNothing).After<T>();
    }
  }
}