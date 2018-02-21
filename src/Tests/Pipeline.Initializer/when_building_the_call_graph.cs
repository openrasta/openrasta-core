using System;
using System.Linq;
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
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void
        a_second_contrib_registering_after_the_first_contrib_that_registers_after_the_boot_initializes_the_call_list_in_the_correct_order(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType,
          new[]
          {
              typeof(SecondIsAfterFirstContributor),
              typeof(FirstIsAfterBootstrapContributor),
              typeof(BootstrapperContributor)
          },
          false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
          {
              typeof(BootstrapperContributor),
              typeof(FirstIsAfterBootstrapContributor),
              typeof(SecondIsAfterFirstContributor)
          },
          (a, b) => a.GetType() == b);
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void can_register_before_ibegin(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType,
          new[]
          {
              typeof(AfterContributor<KnownStages.IBegin>),
              typeof(BeforeContributor<KnownStages.IBegin>),
              typeof(BootstrapperContributor)
          },
          false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
          {
              typeof(BeforeContributor<KnownStages.IBegin>),
              typeof(BootstrapperContributor),
              typeof(AfterContributor<KnownStages.IBegin>)
          },
          (a, b) => b.IsInstanceOfType(a));
    }
    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void multiple_root_nodes_with_known_type_dependency(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType,
        new[]
        {
          typeof(BootstrapperContributor),
          typeof(OperationInvokerContributor),
          typeof(BeforeContributor<KnownStages.IBegin>),
          typeof(BeforeContributor<KnownStages.IOperationExecution>),
        },
        false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
        {
          typeof(BeforeContributor<KnownStages.IBegin>),
          typeof(BootstrapperContributor),
          typeof(BeforeContributor<KnownStages.IOperationExecution>),
          typeof(OperationInvokerContributor),
        },
        (a, b) => b.IsInstanceOfType(a));
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void multiple_root_nodes_with_one_removed_known_type_dependency(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType,
        new[]
        {
          typeof(BootstrapperContributor),
          typeof(OperationInvokerContributor),
          typeof(BeforeContributor<BeforeContributor<KnownStages.IOperationExecution>>),
          typeof(BeforeContributor<KnownStages.IBegin>),
          typeof(BeforeContributor<KnownStages.IOperationExecution>)
        },
        false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
        {
          typeof(BeforeContributor<KnownStages.IBegin>),
          typeof(BootstrapperContributor),
          typeof(BeforeContributor<BeforeContributor<KnownStages.IOperationExecution>>),
          typeof(BeforeContributor<KnownStages.IOperationExecution>),
          typeof(OperationInvokerContributor),
        },
        (a, b) => b.IsInstanceOfType(a));
    }
    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void can_register_after_iend(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType,
          new[]
          {
              typeof(BeforeContributor<KnownStages.IEnd>),
              typeof(AfterContributor<KnownStages.IEnd>),
              typeof(EndContributor)
          },
          false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
          {
              typeof(BeforeContributor<KnownStages.IEnd>),
              typeof(EndContributor),
              typeof(AfterContributor<KnownStages.IEnd>)
          },
          (a, b) => a.GetType() == b);
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void registering_all_the_contributors_results_in_a_correct_call_graph(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType,
          new[]
          {
              typeof(FirstIsAfterBootstrapContributor),
              typeof(SecondIsAfterFirstContributor),
              typeof(ThirdIsBeforeFirstContributor),
              typeof(FourthIsAfterThirdContributor),
              typeof(BootstrapperContributor)
          },
          false);

      pipeline.Contributors.ShouldHaveSameElementsAs(new[]
          {
              typeof(BootstrapperContributor),
              typeof(ThirdIsBeforeFirstContributor),
              typeof(FourthIsAfterThirdContributor),
              typeof(FirstIsAfterBootstrapContributor),
              typeof(SecondIsAfterFirstContributor)
          },
          (a, b) => a.GetType() == b);
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void registering_all_the_contributors_results_in_a_correct_call_graph_topological(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(typeof(TopologicalSortCallGraphGenerator),
          new[]
          {
              typeof(FirstIsAfterBootstrapContributor),
              typeof(SecondIsAfterFirstContributor),
              typeof(ThirdIsBeforeFirstContributor),
              typeof(FourthIsAfterThirdContributor),
              typeof(BootstrapperContributor)
          },
          false);

      var contribs = pipeline.Contributors.Select(c => c.GetType()).ToList();
      int idx<T>() => contribs.IndexOf(typeof(T));


      idx<FirstIsAfterBootstrapContributor>().ShouldBeGreaterThan(
          idx<BootstrapperContributor>());

      idx<SecondIsAfterFirstContributor>().ShouldBeGreaterThan(
          idx<FirstIsAfterBootstrapContributor>());

      idx<ThirdIsBeforeFirstContributor>().ShouldBeLessThan(
          idx<FirstIsAfterBootstrapContributor>());
      idx<FourthIsAfterThirdContributor>().ShouldBeGreaterThan(
          idx<ThirdIsBeforeFirstContributor>());
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void the_call_graph_cannot_be_recursive(Type callGraphGeneratorType)
    {
      Executing(() => CreatePipeline(callGraphGeneratorType,
              new[]
              {
                  typeof(BootstrapperContributor),
                  typeof(RecursiveA),
                  typeof(RecursiveB)
              },
              false))
          .ShouldThrow<RecursionException>();
    }

    [Theory]
    [InlineData(typeof(WeightedCallGraphGenerator))]
    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public void registering_contributors_with_multiple_recursive_notifications_should_be_identified_as_invalid(
        Type callGraphGeneratorType)
    {
      Executing(() => CreatePipeline(callGraphGeneratorType,
              new[]
              {
                  typeof(BootstrapperContributor),
                  typeof(ContributorA),
                  typeof(ContributorB),
                  typeof(ContributorC)
              },
              false))
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