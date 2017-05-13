using System;
using System.Linq;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;
using Shouldly;
using Tests.Pipeline.Initializer.Infrastructure;
using Xunit;

namespace Tests.Pipeline.Initializer
{
  public class when_accessing_the_contributors : initializer_context
  {
    [Theory]
    [InlineData(null)]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void the_contributor_list_always_contains_the_bootstrap_contributor(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType, new Type[] { }, false);

      pipeline.Contributors.OfType<KnownStages.IBegin>()
        .FirstOrDefault()
        .ShouldNotBeNull();
    }
  }
}