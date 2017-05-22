using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using OpenRasta.OperationModel;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Web;
using OpenRasta.Pipeline;
using OpenRasta.Tests.Unit.Infrastructure;
using OperationCreationContributor_Specification;
using Shouldly;

namespace OpenRasta.Tests.Unit.Web.Pipeline.Contributors
{
  namespace CodecSelector
  {
    public class when_no_operation_is_returned : operation_processors_context<RequestCodecSelector>
    {
      [Test]
      public void the_result_is_a_415_error()
      {
        given_processor();
        given_operations(0);

        when_executing_processor();

        Result.ShouldBe(PipelineContinuation.RenderNow);
        //return valueToAnalyse;
        Context.OperationResult.ShouldBeAssignableTo<OperationResult.RequestMediaTypeUnsupported>();
      }

      protected override RequestCodecSelector create_processor()
      {
        return new RequestCodecSelector(Resolver);
      }

      void when_executing_processor()
      {
        when_sending_notification<KnownStages.IOperationFiltering>();
      }
    }
  }

  namespace Filter
  {
  }

  namespace Hydrator
  {
  }

  public abstract class operation_processors_context<TStage> : contributor_context<TStage>
    where TStage : class, IPipelineContributor
  {
    protected void given_processor()
    {
      given_pipeline_contributor(create_processor);
    }

    protected abstract TStage create_processor();

    protected void given_operations(int count)
    {
      var mock = new Mock<IOperationCreator>();
      Context.PipelineData.OperationsAsync = count >= 0
        ? Enumerable.Range(0, count).Select(i => CreateMockOperation()).ToList()
        : null;
    }

    IOperationAsync CreateMockOperation()
    {
      var operation = new Mock<IOperationAsync>();
      operation.Setup(x => x.ToString()).Returns("Fake method");
      operation.SetupGet(x => x.Name).Returns("OperationName");
      return operation.Object;
    }
  }
}