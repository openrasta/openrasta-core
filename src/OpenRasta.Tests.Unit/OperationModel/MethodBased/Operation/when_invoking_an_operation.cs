using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenRasta.Tests.Unit.Infrastructure;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  public class when_invoking_an_operation : operation_context<MockOperationHandler>
  {
    [Test]
    public void an_operation_not_ready_for_invocation_throws_an_exception()
    {
      given_operation("Post", typeof(int), typeof(string));

      Executing(Operation.InvokeAsync).ShouldThrow<InvalidOperationException>();
    }
    [Test]
    public void a_result_is_returned()
    {
      given_operation("Get", typeof(int));

      Operation.InvokeAsync().Result.Count().ShouldBe(1);
      //return valueToAnalyse;
    }
  }

  public class AsyncHandler
  {
    public Task Get()
    {
      return Task.FromResult(1);
    }

    public Task<string> GetText()
    {
      return Task.FromResult("Hello, is it me you're looking for?");
    }
  }
}
