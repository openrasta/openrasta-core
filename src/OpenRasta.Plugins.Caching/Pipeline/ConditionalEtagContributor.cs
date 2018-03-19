using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class ConditionalEtagContributor : ConditionalContributor, IPipelineContributor
  {
    // TODO: Simplify this class, it's unreadable.
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessPostConditional)
        .Before<ConditionalLastModifiedContributor>()
        .And
        .Before<KnownStages.IResponseCoding>();
    }

    static PipelineContinuation ProcessPostConditional(ICommunicationContext context)
    {
      if (!ShouldProcessConditional(context)) return PipelineContinuation.Continue;

      if (InvalidHeaderConbination(context)) return PipelineContinuation.Continue;

      ProcessIf(context, CachingHttpHeaders.IfNoneMatch, ProcessIfNoneMatch);
      ProcessIf(context, CachingHttpHeaders.IfMatch, ProcessIfMatch);
      // we ignore if-none-match for now. Yay!
      return PipelineContinuation.Continue;
    }

    static void ProcessIf(ICommunicationContext context, string requestHeader,
      Action<ICommunicationContext, string, IEnumerable<ETagValidator>> process)
    {
      context.Request.Header(requestHeader, requestIf =>
        context.Response.Header(CachingHttpHeaders.Etag, entityTag =>
          process(context, entityTag, ParseEtags(requestIf))));
    }

    static void ProcessIfNoneMatch(ICommunicationContext context, string entityTag,
      IEnumerable<ETagValidator> validators)
    {
      if (validators.Any(_ => _.Matches(entityTag)))
        NotModified(context);
    }

    static void ProcessIfMatch(ICommunicationContext context, string entityTag, IEnumerable<ETagValidator> validators)
    {
      if (validators.Any(_ => _.Matches(entityTag)) == false)
        NotModified(context);
    }

    static IEnumerable<ETagValidator> ParseEtags(string value)
    {
      return value.Split(',').Select(_ => _.Trim())
        .Select(ETagValidator.TryParse)
        .Where(_ => _ != null);
    }

    static bool ShouldProcessConditional(ICommunicationContext context)
    {
      return context.Response.StatusCode == 200 &&
             !context.Request.Headers.ContainsKey(CachingHttpHeaders.IfRange) &&
             (context.Request.HttpMethod == "HEAD" || context.Request.HttpMethod == "GET");
    }
  }
}