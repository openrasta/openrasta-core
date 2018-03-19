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

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - ioc
    public ILogger Log { get; set; }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessConditional).Before<KnownStages.IResponseCoding>();
    }

    PipelineContinuation ProcessConditional(ICommunicationContext context)
    {
      return !ShouldProcessConditional(context)
        ? PipelineContinuation.Continue
        : IfModifiedSince(context);
    }

    PipelineContinuation IfModifiedSince(ICommunicationContext context)
    {
      var now = context.PipelineData.GetCachingTime();

      context.Request.HeaderDateTimeOffset(CachingHttpHeaders.IfModifiedSince,
        ifModifiedSince => context.Response.HeaderDateTimeOffset(CachingHttpHeaders.LastModified,
          lastModified => ProcessConditional(context, ifModifiedSince, lastModified, now),
          LogIfModifiedSinceWarning));
      return PipelineContinuation.Continue;
    }

    void LogIfModifiedSinceWarning(string erronousHeader)
    {
      Log.WriteWarning("Invalid If-Modified-Since value, not RFC1123 compliant: {0}", CachingHttpHeaders.LastModified,
        erronousHeader);
    }

    static void ProcessConditional(ICommunicationContext context, DateTimeOffset ifModifiedSince,
      DateTimeOffset lastModified, DateTimeOffset now)
    {
      if (lastModified > now) lastModified = now;

      // timer resolution has to be one second, operators wont work.
      if (lastModified - ifModifiedSince < TimeSpan.FromSeconds(1))
        NotModified(context);
    }


    static bool ShouldProcessConditional(ICommunicationContext context)
    {
      return context.Response.StatusCode == 200 &&
             !context.Request.Headers.ContainsKey(CachingHttpHeaders.IfRange) &&
             (context.Request.HttpMethod == "GET" ||
              context.Request.HttpMethod == "HEAD") &&
             !InvalidHeaderConbination(context);
    }
  }
}