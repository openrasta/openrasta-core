using System;
using OpenRasta;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Web;
using Shouldly;
using Tests.Infrastructure;
using Tests.Pipeline.Initializer.Examples;
using Tests.Pipeline.Initializer.Infrastructure;
using Xunit;

namespace Tests.Pipeline.Initializer
{
  public class when_building_the_call_graph : initializer_context
  {
    [Theory]
    [InlineData(null)]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void
      a_second_contrib_registering_after_the_first_contrib_that_registers_after_the_boot_initializes_the_call_list_in_the_correct_order
      (Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType, new[]
      {
        typeof(SecondIsAfterFirstContributor),
        typeof(FirstIsAfterBootstrapContributor)
      }, false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
      {
        typeof(BootstrapperContributor),
        typeof(FirstIsAfterBootstrapContributor),
        typeof(SecondIsAfterFirstContributor)
      }, (a, b) => a.GetType() == b);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    public void registering_all_the_contributors_results_in_a_correct_call_graph(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType, new[]
      {
        typeof(FirstIsAfterBootstrapContributor),
        typeof(SecondIsAfterFirstContributor),
        typeof(ThirdIsBeforeFirstContributor),
        typeof(FourthIsAfterThirdContributor)
      }, false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
      {
        typeof(BootstrapperContributor),
        typeof(ThirdIsBeforeFirstContributor),
        typeof(FourthIsAfterThirdContributor),
        typeof(FirstIsAfterBootstrapContributor),
        typeof(SecondIsAfterFirstContributor)
      }, (a, b) => a.GetType() == b);
    }

    [Fact]
    public void registering_all_the_contributors_results_in_a_correct_call_graph_topological()
    {
      var pipeline = CreatePipeline(typeof(TopologicalSortCallGraphGenerator), new[]
      {
        typeof(FirstIsAfterBootstrapContributor),
        typeof(SecondIsAfterFirstContributor),
        typeof(ThirdIsBeforeFirstContributor),
        typeof(FourthIsAfterThirdContributor)
      }, false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
      {
        typeof(BootstrapperContributor),
        typeof(ThirdIsBeforeFirstContributor),
        typeof(FirstIsAfterBootstrapContributor),
        typeof(SecondIsAfterFirstContributor),
        typeof(FourthIsAfterThirdContributor)
      }, (a, b) => a.GetType() == b);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void the_call_graph_cannot_be_recursive(Type callGraphGeneratorType)
    {
      Executing(() => CreatePipeline(callGraphGeneratorType, new[]
        {
          typeof(RecursiveA), typeof(RecursiveB)
        }, false))
        .ShouldThrow<RecursionException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void registering_contributors_with_multiple_recursive_notifications_should_be_identified_as_invalid(
      Type callGraphGeneratorType)
    {
      Executing(() => CreatePipeline(callGraphGeneratorType, new[]
        {
          typeof(ContributorA),
          typeof(ContributorB),
          typeof(ContributorC)
        }, false))
        .ShouldThrow<RecursionException>();
    }

    static PipelineContinuation DoNothing(ICommunicationContext c)
    {
      return PipelineContinuation.Continue;
    }

    class ContributorA : IPipelineContributor
    {
      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner.Notify(DoNothing).After<KnownStages.IBegin>();
      }
    }

    class ContributorB : IPipelineContributor
    {
      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner.Notify(DoNothing).After<ContributorA>();
        pipelineRunner.Notify(DoNothing).After<ContributorC>();
      }
    }

    class ContributorC : IPipelineContributor
    {
      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner.Notify(DoNothing).After<ContributorB>();
      }
    }

    class FirstIsAfterBootstrapContributor : AfterContributor<KnownStages.IBegin>
    {
    }

    class FourthIsAfterThirdContributor : AfterContributor<ThirdIsBeforeFirstContributor>
    {
    }

    class RecursiveA : IPipelineContributor
    {
      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner.Notify(DoNothing).After<KnownStages.IBegin>().And.After<RecursiveB>();
      }
    }

    class RecursiveB : AfterContributor<RecursiveA>
    {
    }

    class SecondIsAfterFirstContributor : AfterContributor<FirstIsAfterBootstrapContributor>
    {
    }

    class ThirdIsBeforeFirstContributor : BeforeContributor<FirstIsAfterBootstrapContributor>
    {
    }
  }
}