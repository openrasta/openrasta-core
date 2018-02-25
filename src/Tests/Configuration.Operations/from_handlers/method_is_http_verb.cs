using System.Linq;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Configuration.Operations
{
  public class method_is_http_verb : handler_scenario
  {
    public method_is_http_verb()
    {
      given_server_with_handler<Handler>();
      
      operation = metamodel.ResourceRegistrations.Single().Uris.Single().Operations.ShouldHaveSingleItem();
    }

    [Fact]
    public void names_are_correct()
    {
      operation.HttpMethod.ShouldBe("GET");

      operation.Name.ShouldBe("Get");
    }


    OpenRasta.Configuration.MetaModel.OperationModel operation;
    class Handler
    {
      public OperationResult Get() => new OperationResult.OK();
    }
  }
}