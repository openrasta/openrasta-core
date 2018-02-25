using System.Linq;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Configuration.Operations
{
  public class method_has_http_operation_attribute : handler_scenario
  {
    public method_has_http_operation_attribute()
    {
      given_server_with_handler<Handler>();
      
      operation = metamodel.ResourceRegistrations.Single().Uris.Single().Operations.ShouldHaveSingleItem();
    }

    [Fact]
    public void names_are_correct()
    {
      operation.HttpMethod.ShouldBe("DELETE");

      operation.Name.ShouldBe("LetsKillThis");
    }


    OpenRasta.Configuration.MetaModel.OperationModel operation;
    class Handler
    {
      [HttpOperation("DELETE")]
      public OperationResult LetsKillThis() => new OperationResult.OK();
    }
  }
}