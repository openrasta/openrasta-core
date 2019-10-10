using System;
using OpenRasta.Pipeline;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  class ContributorThrowingAfter<T> : IPipelineContributor where T : IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner
        .Notify(ctx => throw new InvalidOperationException("This naughty contrib throws"))
        .After<T>();
    }
  }
}