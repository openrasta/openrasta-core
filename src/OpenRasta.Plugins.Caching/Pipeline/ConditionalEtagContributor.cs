using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class ConditionalEtagContributor : ConditionalContributor, IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessPostConditional)
        .Before<ConditionalLastModifiedContributor>()
        .And
        .Before<KnownStages.IResponseCoding>();
    }

    PipelineContinuation ProcessPostConditional(ICommunicationContext context)
    {
      if (!ShouldProcessConditional(context)) return PipelineContinuation.Continue;

      if (InvalidHeaderConbination(context)) return PipelineContinuation.Continue;

      ProcessIf(context, CachingHttpHeaders.IF_NONE_MATCH, ProcessIfNoneMatch);
      ProcessIf(context, CachingHttpHeaders.IF_MATCH, ProcessIfMatch);
      // we ignore if-none-match for now. Yay!
      return PipelineContinuation.Continue;
    }

    void ProcessIf(ICommunicationContext context, string requestHeader,
      Action<ICommunicationContext, string, IEnumerable<ETagValidator>> process)
    {
      context.Request.Header(requestHeader, requestIf =>
        context.Response.Header(CachingHttpHeaders.ETAG, entityTag =>
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

    IEnumerable<ETagValidator> ParseEtags(string value)
    {
      return value.Split(',').Select(_ => _.Trim())
        .Select(ETagValidator.TryParse)
        .Where(_ => _ != null);
    }

    bool ShouldProcessConditional(ICommunicationContext context)
    {
      return context.Response.StatusCode == 200 &&
             !context.Request.Headers.ContainsKey(CachingHttpHeaders.IF_RANGE) &&
             (context.Request.HttpMethod == "HEAD" || context.Request.HttpMethod == "GET");
    }
  }
}