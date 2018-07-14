using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using Shouldly;
using Tests.Plugins.Hydra.Examples;
using Xunit;

namespace Tests.Plugins.Hydra
{
  public class apiEndpoint
  {
    InMemoryHost server;

    public apiEndpoint()
    {
      
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra();

        ResourceSpace.Has
          .ResourcesOfType<List<Event>>()
          .Vocabulary(ExampleVocabularies.Events)
          .AtUri("/events")
          .Collection();

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary(ExampleVocabularies.Events)
          .AtUri("/events/{id}");
      });
    }

  }
}