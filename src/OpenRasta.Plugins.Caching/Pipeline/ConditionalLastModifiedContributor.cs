using System;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Plugins.Caching.Configuration;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class ConditionalLastModifiedContributor : ConditionalContributor, IPipelineContributor
  {
    public ConditionalLastModifiedContributor()
    {
      Log = NullLogger.Instance;
    }

    public ILogger Log { get; set; }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessConditional).Before<KnownStages.IResponseCoding>();
    }

    PipelineContinuation ProcessConditional(ICommunicationContext context)
    {
      if (!ShouldProcessConditional(context)) return PipelineContinuation.Continue;


      return IfModifiedSince(context);
    }

    PipelineContinuation IfModifiedSince(ICommunicationContext context)
    {
      var now = context.PipelineData.GetCachingTime();

      context.Request.HeaderDateTimeOffset(CachingHttpHeaders.IF_MODIFIED_SINCE,
        ifModifiedSince => context.Response.HeaderDateTimeOffset(CachingHttpHeaders.LAST_MODIFIED,
          lastModified => ProcessConditional(context, ifModifiedSince, lastModified, now),
          LogIfModifiedSinceWarning));
      return PipelineContinuation.Continue;
    }

    void LogIfModifiedSinceWarning(string erronousHeader)
    {
      Log.WriteWarning("Invalid If-Modified-Since value, not RFC1123 compliant: {0}", CachingHttpHeaders.LAST_MODIFIED,
        erronousHeader);
    }

    static void ProcessConditional(ICommunicationContext context, DateTimeOffset ifModifiedSince,
      DateTimeOffset lastModified, DateTimeOffset now)
    {
      if (lastModified > now) lastModified = now;

      // timer resolution has to be one second, operators wont work.
      if (lastModified - ifModifiedSince < TimeSpan.FromSeconds(value: 1)) NotModified(context);
    }


    bool ShouldProcessConditional(ICommunicationContext context)
    {
      return context.Response.StatusCode == 200 &&
             !context.Request.Headers.ContainsKey(CachingHttpHeaders.IF_RANGE) &&
             (context.Request.HttpMethod == "GET" ||
              context.Request.HttpMethod == "HEAD") &&
             !InvalidHeaderConbination(context);
    }
  }
}