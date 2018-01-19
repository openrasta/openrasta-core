using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Legacy.Configuration.Manual
{
  public class containing_using_block_expecting_config_end_on_dispose
  {
    readonly InMemoryHost server;
    ConfigurationRelyingOnUsingDisposingBehaviour config;

    public containing_using_block_expecting_config_end_on_dispose()
    {
      var resolver = new InternalDependencyResolver();
      config = new ConfigurationRelyingOnUsingDisposingBehaviour(resolver);
      server = new InMemoryHost(
        config,
        resolver);
      
    }

    [Fact]
    public void configuration_is_successful()
    {
      server.Resolver.Resolve<ExampleService>().ShouldNotBeNull();
    }

    [Fact]
    public void usage_after_dispose_is_correct()
    {
      config.Resolved.ShouldNotBeNull();
    }
    
    
    class ConfigurationRelyingOnUsingDisposingBehaviour : IConfigurationSource
    {
      readonly IDependencyResolver resolver;
      public ExampleService Resolved;

      public ConfigurationRelyingOnUsingDisposingBehaviour(IDependencyResolver resolver)
      {
        this.resolver = resolver;
      }
      public void Configure()
      {
#pragma warning disable 618
        using (OpenRastaConfiguration.Manual)
        {
          ResourceSpace.Uses.Dependency(context => context.Register(() => new ExampleService()));
        }

        Resolved = resolver.Resolve<ExampleService>();
#pragma warning restore 618
      }
    }
  }
}