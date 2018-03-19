using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Pipeline;
using OpenRasta.Plugins.Caching.Configuration;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class EntityLastModified : IPipelineContributor
  {
    readonly IMetaModelRepository _config;

    public EntityLastModified(IMetaModelRepository config)
    {
      _config = config;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(PostExecution).After<KnownStages.ICodecResponseSelection>()
        .And.Before<ConditionalLastModifiedContributor>();
    }

    PipelineContinuation PostExecution(ICommunicationContext context)
    {
      if (!ShouldSendLastModified(context)) return PipelineContinuation.Continue;
      var now = context.PipelineData.GetCachingTime();
      var matchingRegistration =
        _config.ResourceRegistrations.FindAll(context.OperationResult.ResponseResource.GetType());

      var reader = GetLastModifiedMapper(matchingRegistration);

      var lastModified = reader(context.OperationResult.ResponseResource);
      if (lastModified == null) return PipelineContinuation.Continue;
      if (lastModified > now) lastModified = now;

      context.Response.Headers[CachingHttpHeaders.LastModified] = lastModified.Value.ToUniversalTime().ToString("R");

      return PipelineContinuation.Continue;
    }

    static Func<object, DateTimeOffset?> GetLastModifiedMapper(IEnumerable<ResourceModel> matchingRegistration)
    {
      DateTimeOffset? nullReader(object resource) => null;
      var reader = matchingRegistration.Select(_ => _.GetLastModifiedMapper())
        .Aggregate((Func<object, DateTimeOffset?>) nullReader,
          (src, read) => resource => src(resource) ?? read(resource));
      return reader;
    }

    static bool ShouldSendLastModified(ICommunicationContext context)
    {
      return context.OperationResult.StatusCode == 200 &&
             context.OperationResult.ResponseResource != null &&
             !context.Response.Headers.ContainsKey(CachingHttpHeaders.LastModified);
    }
  }
}