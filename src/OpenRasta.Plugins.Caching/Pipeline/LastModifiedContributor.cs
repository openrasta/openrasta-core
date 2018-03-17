using System;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class LastModifiedContributor : IPipelineContributor
  {
    readonly IMetaModelRepository _config;
    readonly ILogger _log;

    public LastModifiedContributor(IMetaModelRepository config, ILogger log)
    {
      _config = config;
      _log = log;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(PostExecution).After<KnownStages.IOperationExecution>()
        .And.Before<KnownStages.IOperationResultInvocation>();
      pipelineRunner.Notify(RewriteResult).After<KnownStages.IOperationResultInvocation>()
        .And.Before<KnownStages.IResponseCoding>();
    }

    PipelineContinuation RewriteResult(ICommunicationContext arg)
    {
      object process;
      if (arg.PipelineData.TryGetValue(Keys.REWRITE_TO_304, out process) && (bool) process)
      {
        arg.OperationResult = new OperationResult.NotModified();
        arg.Response.Entity.Instance = null;
        arg.Response.StatusCode = 304;
        arg.Response.Entity.ContentLength = 0;
      }

      return PipelineContinuation.Continue;
    }

    PipelineContinuation PostExecution(ICommunicationContext context)
    {
      if (context.OperationResult.ResponseResource == null)
        return PipelineContinuation.Continue;


      var registration =
        _config.ResourceRegistrations.SingleOrDefault(_ => _.ResourceKey == context.PipelineData.ResourceKey);
      if (registration == null || registration.Properties.ContainsKey(Keys.LAST_MODIFIED) == false)
        return PipelineContinuation.Continue;
      var now = ServerClock.UtcNow();
      var mapper = (Func<object, DateTimeOffset?>) registration.Properties[Keys.LAST_MODIFIED];
      var lastModified = mapper(context.OperationResult.ResponseResource);

      if (lastModified > now)
        lastModified = now;
      var ifModifiedSinceHeader = context.Request.Headers["if-modified-since"];
      if (NoIncompatiblePreconditions(context) &&
          ifModifiedSinceHeader != null)
      {
        DateTimeOffset modifiedSince;
        var validIfModifiedSince = DateTimeOffset.TryParse(
          ifModifiedSinceHeader,
          out modifiedSince);
        if (!validIfModifiedSince)
          _log.WriteWarning("Invalid If-Modified-Since value, not RFC1123 compliant: {0}", ifModifiedSinceHeader);
        else if (lastModified <= modifiedSince)
          context.PipelineData[Keys.REWRITE_TO_304] = true;
      }

      if (lastModified != null)
        WriteLastModifiedHeader(context, lastModified);
      return PipelineContinuation.Continue;
    }

    bool NoIncompatiblePreconditions(ICommunicationContext context)
    {
      return !context.Response.Headers.ContainsKey("last-modified") &&
             !context.Request.Headers.ContainsKey("if-match") &&
             !context.Request.Headers.ContainsKey("if-match") &&
             !context.Request.Headers.ContainsKey("if-none-match") &&
             !context.Request.Headers.ContainsKey("if-range") &&
             !context.Request.Headers.ContainsKey("range");
    }

    void WriteLastModifiedHeader(ICommunicationContext arg, DateTimeOffset? lastModified)
    {
      arg.Response.Headers["last-modified"] = lastModified.Value.ToUniversalTime().ToString("R");
    }
  }
}