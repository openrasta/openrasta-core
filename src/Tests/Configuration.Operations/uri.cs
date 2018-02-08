using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Configuration.Operations
{
  public class uri
  {
    IMetaModelRepository metamodel;

    public uri()
    {
      using (var server = new InMemoryHost(() =>
      {
        ResourceSpace.Has.ResourcesNamed("test")
          .AtUri("/resource")
          .HandledBy<Handler>();
      }))
      {
        metamodel = server.Resolver.Resolve<IMetaModelRepository>();  
      }
    }

    [Fact]
    public void one_operation_is_defined_for_method_name()
    {
      var resource = metamodel.ResourceRegistrations.ByName("test").Single();
      
      var operation = resource.Uris.Single()
        .Operations.ShouldHaveSingleItem();
      operation.Name.ShouldBe("Get");
      operation.HttpMethod.ShouldBe("GET");
    }
    
    
    class Handler
    {
      public OperationResult Get() => new OperationResult.OK();
    }
  }

  public static class MetaModelExtensions
  {
    public static IEnumerable<ResourceModel> ByName(this IEnumerable<ResourceModel> resources, string name)
    {
      return resources.Where(r => (string) r.ResourceKey == name);
    }
  }
}