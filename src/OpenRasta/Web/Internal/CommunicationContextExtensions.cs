using System;
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
      context.OperationResult = new OperationResult.InternalServerError
      {
        Title = "The request could not be processed because of a fatal error. See log below.",
        ResponseResource = context.ServerErrors
      };
      context.PipelineData.ResponseCodec = null;
      context.Response.StatusCode = 500;
      context.Response.Entity.Instance = context.ServerErrors;
      context.Response.Entity.Codec = null;
      context.Response.Entity.ContentLength = null;
    }
  }
}