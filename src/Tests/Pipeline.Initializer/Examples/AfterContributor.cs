using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace Tests.Pipeline.Initializer.Examples
{
  public class AfterContributor<T> : IPipelineContributor where T : IPipelineContributor
  {
    static PipelineContinuation DoNothing(ICommunicationContext c)
    {
      return PipelineContinuation.Continue;
    }

    public virtual void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(DoNothing).After<T>();
    }
  }
}