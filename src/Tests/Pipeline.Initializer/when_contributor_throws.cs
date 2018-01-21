using System;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;
using Shouldly;
using Tests.Pipeline.Initializer.Infrastructure;
using Xunit;

namespace Tests.Pipeline.Initializer
{
  public class when_contributor_throws : initializer_context
  {
    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task error_is_collected_and_500_returned(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType, new[]
      {
        typeof(FakeUriMatcher),
        typeof(ContributorThatThrows),
        typeof(FakeOperationResultInvoker)
      }, false);

      var context = new InMemoryCommunicationContext();

      await pipeline.RunAsync(context);
      context.Response.StatusCode.ShouldBe(500);
      context.ServerErrors.Count.ShouldBe(1);
    }

    class FakeUriMatcher : KnownStages.IUriMatching
    {
      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner.NotifyAsync(env => Task.FromResult(PipelineContinuation.Continue)).After<KnownStages.IBegin>();
      }
    }

    class FakeOperationResultInvoker : KnownStages.IOperationResultInvocation
    {
      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner.Notify(env => PipelineContinuation.Continue).After<ContributorThatThrows>();
      }
    }

    class ContributorThatThrows : IPipelineContributor
    {
      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner
          .Notify(ctx => throw new InvalidOperationException("This naughty contrib throws"))
          .After<KnownStages.IUriMatching>();
      }
    }
  }
}