using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public static class ContextExtensions
  {
    public static PipelineContinuation Respond(this ICommunicationContext env, OperationResult result)
    {
      env.Response.Entity.Instance = result;
      return PipelineContinuation.RenderNow;
    }

    public static PipelineContinuation Respond<T>(this ICommunicationContext env) where T:OperationResult,new()
    {
      env.Response.Entity.Instance = new T();
      return PipelineContinuation.RenderNow;
    }
  }
}