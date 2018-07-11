using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using Shouldly;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra
{
  public class endpoint
  {
    InMemoryHost server;

    public endpoint()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra();
      });
    }

    [Fact]
    public async Task context_is_defined()
    {
      var response = await server.GetJsonLd("/");
      response["@context"].ShouldBe("http://localhost/.hydra/context.jsonld");
    }  
  }
}