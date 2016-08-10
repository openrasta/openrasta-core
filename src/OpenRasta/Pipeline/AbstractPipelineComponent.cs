using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public abstract class AbstractPipelineComponent : IPipelineMiddleware
  {
    protected IPipelineComponent Next { get; set; }
    protected Func<ICommunicationContext, Task<PipelineContinuation>> Contributor { get; set; }
    public abstract Task Invoke(ICommunicationContext env);

    protected AbstractPipelineComponent(Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor)
    {
      Contributor = singleTapContributor;

    }

    public virtual IPipelineComponent Build(IPipelineComponent next)
    {
      Next = next;
      return this;
    }

    protected async Task<PipelineContinuation> InvokeSingleTap(ICommunicationContext env)
    {
      PipelineContinuation newState;
      try
      {
        newState = await Contributor(env);
      }
      catch (Exception error)
      {
        newState = PipelineContinuation.Abort;

        env.ServerErrors.Add(new Error()
        {
          Exception = error,
          Title = $"A contributor has raised '{error.GetType().Name}'"
        });
      }
      return newState;
    }
  }
}
