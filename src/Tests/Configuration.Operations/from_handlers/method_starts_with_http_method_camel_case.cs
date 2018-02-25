using System.Linq;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Configuration.Operations
{
  public class method_starts_with_http_method_camel_case : handler_scenario
  {
    public method_starts_with_http_method_camel_case()
    {
      given_server_with_handler<Handler>();
      
      operation = metamodel.ResourceRegistrations.Single().Uris.Single().Operations.ShouldHaveSingleItem();
    }

    [Fact]
    public void names_are_correct()
    {
      operation.HttpMethod.ShouldBe("GET");

      operation.Name.ShouldBe("GetData");
    }


    OpenRasta.Configuration.MetaModel.OperationModel operation;
    class Handler
    {
      public OperationResult GetData() => new OperationResult.OK();
    }
  }
}