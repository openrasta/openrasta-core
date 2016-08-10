using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public abstract class AbstractPipelineComponent : IPipelineComponent
  {
    protected IPipelineComponent Next { get; set; }
    protected Func<ICommunicationContext, Task<PipelineContinuation>> Contributor { get; set; }
    public abstract Task Invoke(ICommunicationContext env);

    public AbstractPipelineComponent(Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor)
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

    public static Task Abort(ICommunicationContext env)
    {
      env.OperationResult = new OperationResult.InternalServerError
      {
        Title = "The request could not be processed because of a fatal error. See log below.",
        ResponseResource = env.ServerErrors
      };
      env.PipelineData.ResponseCodec = null;
      env.Response.Entity.Instance = env.ServerErrors;
      env.Response.Entity.Codec = null;
      env.Response.Entity.ContentLength = null;
      return Task.FromResult(0);
    }
  }
}