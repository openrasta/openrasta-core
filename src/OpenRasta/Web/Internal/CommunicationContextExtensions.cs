using System;
using System.Linq;
using OpenRasta.Pipeline;

namespace OpenRasta.Web.Internal
{
  public static class CommunicationContextExtensions
  {
    public static Uri GetRequestUriRelativeToRoot(this ICommunicationContext context)
    {
      return context.ApplicationBaseUri
        .EnsureHasTrailingSlash()
        .MakeRelativeUri(context.Request.Uri)
        .MakeAbsolute("http://localhost");
    }

    public static void Abort(this ICommunicationContext context, Exception e = null)
    {
#pragma warning disable 618
      context.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;
#pragma warning restore 618
      if (e != null)
        context.ServerErrors.Add(new Error {Exception = e});

      context.SetOperationResultToServerErrors();
      context.PipelineData.ResponseCodec = null;
      context.Response.StatusCode = 500;
      context.Response.Entity.Instance = context.ServerErrors.ToList();
      context.Response.Entity.Codec = null;
      context.Response.Entity.ContentLength = null;
    }

    public static void SetOperationResultToServerErrors(this ICommunicationContext env)
    {
      env.OperationResult =
        new OperationResult.InternalServerError()
        {
          Title = "Errors happened while executing the request",
          ResponseResource = env.ServerErrors.ToList(),
          Description = $"Errors happened while executing the request: {Environment.NewLine}" +
                        string.Concat(env.ServerErrors.Select(error => $"{error}{Environment.NewLine}"))
        };
    }
  }
}