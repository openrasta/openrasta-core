using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
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

    public apiDocumentation()
    {
      
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra();

        ResourceSpace.Has.ResourcesOfType<Customer>()
          .Vocabulary(ExampleVocabularies.ExampleApp);

      });
    }
  
    [Fact]
    public void custom_class_is_defined()
    {
      body["supportedClass"][0]["@id"].Value<string>().ShouldBe("ev:Customer");
      body["supportedClass"][0]["@type"].Value<string>().ShouldBe("hydra:Class");
    }

    [Fact]
    public void custom_class_properties_are_defined()
    {
      body["supportedClass"][0]["supportedProperty"][0]["property"]["@id"].ShouldBe("ev:Customer/name");
      body["supportedClass"][0]["supportedProperty"][0]["property"]["range"].ShouldBe("xsd:string");
      body["supportedClass"][0]["supportedProperty"][0]["property"]["@type"].ShouldBe("rdf:Property");
    }
    
    public async Task InitializeAsync()
    {
      (_, body) = await server.GetJsonLd("/.hydra/documentation.jsonld");
    }

    public Task DisposeAsync()
    {
      return Task.CompletedTask;
    }
  }
}