using System;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Pipeline.Contributors;
using Shouldly;
using Tests.Pipeline.Initializer.Infrastructure;
using Xunit;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  public abstract class pipeline_building_context : initializer_context
  {
    public Type[] Contributors { get; set; }
    protected InMemoryCommunicationContext Context { get; }

    protected pipeline_building_context()
    {
      Context = new InMemoryCommunicationContext();
    }


    protected async Task RunPipeline(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType, Contributors.Concat(new[]
      {
        typeof(RequestResponseDisposer),
        typeof(PostExecuteMarkerContributor)
      }).ToArray(), opt=>opt.OpenRasta.Pipeline.Validate = false);
      await pipeline.RunAsync(Context);
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task always_returns_500(Type callGraphGeneratorType)
    {
      await RunPipeline(callGraphGeneratorType);

      var is500 = Context.Response.StatusCode == 500 ||
                  Context.OperationResult.IsServerError;
      is500.ShouldBeTrue();
    }


    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task alway_call_post_execute_contributor(Type callGraphGeneratorType)
    {
      await RunPipeline(callGraphGeneratorType);

      Context.PipelineData.ContainsKey(nameof(PostExecuteMarkerContributor)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task always_marks_pipeline_as_finished_at_the_end(Type callGraphGeneratorType)
    {
      await RunPipeline(callGraphGeneratorType);

#pragma warning disable 618
      Context.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Finished);
#pragma warning restore 618
    }
  }
}