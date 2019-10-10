using System;
using System.Threading.Tasks;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Pipeline.Contributors;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  public class throws_in_request_phase : pipeline_building_context
  {
    public throws_in_request_phase()
    {
      Contributors = new[]
      {
        typeof(PreExecutingContributor),
        typeof(RequestPhaseContributor),
        typeof(ContributorThrowingAfter<RequestPhaseContributor>),
        typeof(ContributorExecutingAfter<ContributorThrowingAfter<RequestPhaseContributor>>)
      };
    }
    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task has_request_error(Type callGraphGeneratorType)
    {
      await RunPipeline(callGraphGeneratorType);

      Context.ServerErrors.Count.ShouldBe(1);
    }

  }
}