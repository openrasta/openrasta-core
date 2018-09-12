using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Examples;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra
{
  public class apiDocumentation : IAsyncLifetime
  {
    readonly InMemoryHost server;
    JToken body;
    JToken customer;

    public apiDocumentation()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra(opt=>opt.Vocabulary = ExampleVocabularies.ExampleApp.Uri.ToString());

        ResourceSpace.Has.ResourcesOfType<CreateAction>().Vocabulary(Vocabularies.SchemaDotOrg);
        
        ResourceSpace.Has.ResourcesOfType<Customer>()
          .Vocabulary(ExampleVocabularies.ExampleApp.Uri.ToString())
          .SupportedOperation(new Operation {Method = "POST", Expects = "schema:Event"})
          .SupportedOperation(new CreateAction {Method = "POST", Expects = "schema:Person"});

      });
    }
  
    [Fact]
    public void custom_class_is_defined()
    {
      customer["@type"].Value<string>().ShouldBe("hydra:Class");
    }

    [Fact]
    public void custom_class_properties_are_defined()
    {
      
      customer["supportedProperty"][0]["property"]["@id"].ShouldBe("Customer/name");
      customer["supportedProperty"][0]["property"]["range"].ShouldBe("xsd:string");
      customer["supportedProperty"][0]["property"]["@type"].ShouldBe("rdf:Property");
    }

    [Fact]
    public void supported_operations_are_defined()
    {
      customer["supportedOperation"][0]["@type"].ShouldBe("hydra:Operation");
      customer["supportedOperation"][0]["method"].ShouldBe("POST");
      customer["supportedOperation"][0]["expects"].ShouldBe("schema:Event");
    }
    [Fact]
    public void specific_operations_are_defined()
    {

      customer["supportedOperation"][1]["@type"].ShouldBe("schema:CreateAction");
      customer["supportedOperation"][1]["method"].ShouldBe("POST");
      customer["supportedOperation"][1]["expects"].ShouldBe("schema:Person");
    }
    public async Task InitializeAsync()
    {
      (_, body) = await server.GetJsonLd("/.hydra/documentation.jsonld");
      customer = body["supportedClass"].Single(c=>c["@id"].Value<string>() == "Customer");

    }

    public Task DisposeAsync()
    {
      return Task.CompletedTask;
    }
  }
}