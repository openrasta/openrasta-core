using OpenRasta.Concordia;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Pipeline.Contributors;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.Diagnostics
{
  public class disabled
  {
    [Fact]
    public void logging_is_not_injected()
    {
      IDependencyResolver resolver = new InternalDependencyResolver();
      resolver.AddDependency<IGenerateCallGraphs, TopologicalSortCallGraphGenerator>();
      resolver.AddDependency<IPipelineContributor, BootstrapperContributor>();
      resolver.AddDependency<IPipelineContributor, First>();
      var initialiser = new ThreePhasePipelineInitializer();

      var props = new StartupProperties
      {
        OpenRasta =
        {
          Diagnostics = {TracePipelineExecution = false},
          Pipeline = {Validate = false},
          Factories = { Resolver = resolver }
        }
      };

      var pipeline = initialiser
        .Initialize(props);

      pipeline.MiddlewareFactories
        .ShouldNotContain(factory => factory is LoggingMiddlewareFactory);
    }
  }
}