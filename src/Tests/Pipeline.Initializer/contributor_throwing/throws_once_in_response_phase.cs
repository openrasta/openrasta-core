using System;
using System.Threading.Tasks;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Pipeline.Contributors;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  public class throws_once_in_response_phase : pipeline_building_context
  {
    public throws_once_in_response_phase()
    {
      Contributors = new[]
      {
        typeof(PreExecutingContributor),
        typeof(RequestPhaseContributor),
        typeof(ResponsePhaseContributor),
        typeof(ContributorThrowingOnceAfter<ResponsePhaseContributor>)
      };
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task not_in_catastrophic_failure(Type callGraphGeneratorType)
    {
      await RunPipeline(callGraphGeneratorType);

      Context.PipelineData.ShouldNotContainKey("skipToCleanup");
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task always_has_server_error(Type callGraphGeneratorType)
    {
      await RunPipeline(callGraphGeneratorType);

      Context.ServerErrors.Count.ShouldBe(1);
    }
  }

  public class throws_always_in_response_phase : pipeline_building_context
  {
    public throws_always_in_response_phase()
    {
      Contributors = new[]
      {
        typeof(PreExecutingContributor),
        typeof(RequestPhaseContributor),
        typeof(ResponsePhaseContributor),
        typeof(ContributorThrowingAfter<ResponsePhaseContributor>)
      };
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task is_in_catastrophic_failure(Type callGraphGeneratorType)
    {
      await RunPipeline(callGraphGeneratorType);

      Context.PipelineData.ShouldContainKey("skipToCleanup");
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task always_has_server_error(Type callGraphGeneratorType)
    {
      await RunPipeline(callGraphGeneratorType);

      Context.ServerErrors.Count.ShouldBe(2);
    }
  }
}