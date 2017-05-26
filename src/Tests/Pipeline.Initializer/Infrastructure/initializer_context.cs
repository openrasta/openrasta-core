using System;
using OpenRasta.Concordia;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Pipeline.Contributors;
using Tests.Infrastructure;

namespace Tests.Pipeline.Initializer.Infrastructure
{
  public abstract class initializer_context : context
  {
    protected static IPipelineAsync CreatePipeline(
      Type callGraphGeneratorType,
      Type[] contributorTypes,
      bool validate = true)
    {
      var resolver = new InternalDependencyResolver();
      resolver.AddDependencyInstance<IDependencyResolver>(resolver);
      resolver.AddDependency<IPipelineContributor, BootstrapperContributor>();
      resolver.AddDependency<IPipelineInitializer, ThreePhasePipelineInitializer>();

      if (callGraphGeneratorType != null)
      {
        resolver.AddDependency(typeof(IGenerateCallGraphs), callGraphGeneratorType,
          DependencyLifetime.Singleton);
      }

      foreach (var type in contributorTypes)
        resolver.AddDependency(typeof(IPipelineContributor), type, DependencyLifetime.Singleton);

      var runner = resolver.Resolve<IPipelineInitializer>();

      return runner.Initialize(new StartupProperties
      {
        OpenRasta =
        {
          Pipeline = {Validate = validate},
          Factories = {Resolver = resolver}
        }
      });
    }
  }
}