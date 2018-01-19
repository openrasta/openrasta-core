using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Legacy.Configuration.Manual
{
  public class containing_using_block
  {
    readonly InMemoryHost server;

    public containing_using_block()
    {
      server = new InMemoryHost(new ConfigurationSourceWithUsingBlock());
    }

    [Fact]
    public void configuration_is_successful()
    {
      server.Resolver.Resolve<ExampleService>().ShouldNotBeNull();
    }


    class ConfigurationSourceWithUsingBlock : IConfigurationSource
    {
      public void Configure()
      {
#pragma warning disable 618
        using (OpenRastaConfiguration.Manual)
        {
          ResourceSpace.Uses.Dependency(context => context.Register(() => new ExampleService()));
        }
#pragma warning restore 618
      }
    }
  }
}