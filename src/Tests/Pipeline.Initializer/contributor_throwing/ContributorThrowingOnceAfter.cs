using System;
using OpenRasta.Pipeline;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  class ContributorThrowingOnceAfter<T> : IPipelineContributor where T : IPipelineContributor
  {
    int count;

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner
        .Notify(ctx =>
        {
          if (++count <= 1)
            throw new InvalidOperationException("This naughty contrib throws");
          return PipelineContinuation.Continue;
        })
        .After<T>();
    }
  }
}