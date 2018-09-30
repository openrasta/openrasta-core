using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Examples;
using Tests.Plugins.Hydra.Implementation;
using Tests.Plugins.Hydra.Utf8Json;
using Xunit;
using Customer = Tests.Plugins.Hydra.Examples.Customer;

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
        ResourceSpace.Uses.Hydra(opt =>
        {
          opt.Vocabulary = ExampleVocabularies.ExampleApp.Uri.ToString();
          opt.Serializer = ctx => ctx.Transient(() => new PreCompiledUtf8JsonSerializer()).As<IMetaModelHandler>();
        });

        ResourceSpace.Has.ResourcesOfType<CreateAction>().Vocabulary(Vocabularies.SchemaDotOrg);

        ResourceSpace.Has.ResourcesOfType<Customer>()
          .Vocabulary(ExampleVocabularies.ExampleApp.Uri.ToString())
          .SupportedOperation(new Operation {Method = "POST", Expects = "schema:Event"})
          .SupportedOperation(new CreateAction {Method = "POST", Expects = "schema:Person"});

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary(ExampleVocabularies.ExampleApp.Uri.ToString());

        ResourceSpace.Has.ResourcesNamed("Ignored");
      }, startup: new StartupProperties(){OpenRasta = { Errors = {  HandleAllExceptions = false,HandleCatastrophicExceptions = false}}});
      
    }

    [Fact]
    public void has_all_classes()
    {
      var customerClass = body["supportedClass"].Single(c => c["@id"].Value<string>() == "Customer");
      var eventClass =  body["supportedClass"].Single(c => c["@id"].Value<string>() == "Event");
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
      var op = customer["supportedOperation"].Single(o => o["@type"].Value<string>() == "hydra:Operation");
      
      op["method"].ShouldBe("POST");
      op["expects"].ShouldBe("schema:Event");
    }

    [Fact]
    public void specific_operations_are_defined()
    {
      var op = customer["supportedOperation"].Single(o => o["@type"].Value<string>() == "schema:CreateAction");
      
      op["method"].ShouldBe("POST");
      op["expects"].ShouldBe("schema:Person");
    }

    public async Task InitializeAsync()
    {
      (_, body) = await server.GetJsonLd("/.hydra/documentation.jsonld");
      customer = body["supportedClass"].Single(c => c["@id"].Value<string>() == "Customer");
    }

    public Task DisposeAsync()
    {
      return Task.CompletedTask;
    }
  }
}