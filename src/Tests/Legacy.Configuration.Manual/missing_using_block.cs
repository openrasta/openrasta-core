using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Legacy.Configuration.Manual
{
  public class missing_using_block
  {
    readonly InMemoryHost server;

    public missing_using_block()
    {
      server = new InMemoryHost(new ConfigurationSourceWithoutUsingBlock());
      
    }

    [Fact]
    public void configuration_is_successful()
    {
      server.Resolver.Resolve<ExampleService>().ShouldNotBeNull();
    }


    class ConfigurationSourceWithoutUsingBlock : IConfigurationSource
    {
      public void Configure()
      {
        ResourceSpace.Uses.Dependency(context => context.Register(() => new ExampleService()));
      }
    }
  }
}