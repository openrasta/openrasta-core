using System;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Web;
using Shouldly;
using Tests.Pipeline.Initializer.Infrastructure;
using Xunit;

namespace Tests.Pipeline.Initializer
{
  public class when_executing_the_pipeline : initializer_context
  {
    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task contributors_get_executed(Type callGraphGeneratorType)
    {
      System.Diagnostics.Debug.WriteLine("yo");
      var pipeline = CreatePipeline(callGraphGeneratorType, new[]
      {
        typeof(WasCalledContributor)
      }, false);

      await pipeline.RunAsync(new InMemoryCommunicationContext());
      WasCalledContributor.WasCalled.ShouldBeTrue();
    }

    class WasCalledContributor : IPipelineContributor
    {
      public static bool WasCalled;

      static PipelineContinuation Do(ICommunicationContext context)
      {
        WasCalled = true;
        return PipelineContinuation.Continue;
      }

      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner.Notify(Do).After<KnownStages.IBegin>();
      }
    }
  }
}