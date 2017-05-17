using System.Linq;
using NUnit.Framework;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  public class when_using_async_operation_with_return : operation_context<AsyncHandler>
  {
    [Test]
    public void operation_is_invoked()
    {
      given_operation("GetText");
      Operation.InvokeAsync().Result.LegacyShouldHaveCountOf(1)
        .First().Value.LegacyShouldBe("Hello, is it me you're looking for?");
    }
  }

  public class when_using_async_operation_no_return : operation_context<AsyncHandler>
  {

    [Test]
    public void task_executes()
    {
      given_operation("Get");
      Operation.InvokeAsync().Result.ShouldBeEmpty();
    }
  }
}
