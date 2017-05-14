using System;
using System.Linq;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;
using Shouldly;
using Tests.Pipeline.Initializer.Examples;
using Tests.Pipeline.Initializer.Infrastructure;
using Xunit;

namespace Tests.Pipeline.Initializer
{
  public class when_creating_the_pipeline : initializer_context
  {
    [Theory]
    [InlineData(null)]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void a_registered_contributor_gets_initialized_and_is_part_of_the_contributor_collection(
      Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType, new[]
      {
        typeof(DummyContributor)
      }, false);
      pipeline.Contributors.OfType<DummyContributor>()
        .FirstOrDefault()
        .ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void valid_pipeline_is_required(Type callGraphGeneratorType)
    {
      Executing(() => CreatePipeline(callGraphGeneratorType, new[]
        {
          typeof(DummyContributor)
        }))
        .ShouldThrow<DependentContributorMissingException>()
        .ContributorTypes
        .Count()
        .ShouldBe(typeof(KnownStages).GetNestedTypes().Length - 1);
    }

    class DummyContributor : AfterContributor<KnownStages.IBegin>
    {
    }
  }

}