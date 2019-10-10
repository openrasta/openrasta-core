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
      Action<StartupProperties> options = null)
    {
      var resolver = new InternalDependencyResolver();
      resolver.AddDependencyInstance<IDependencyResolver>(resolver);
      resolver.AddDependency<IPipelineInitializer, ThreePhasePipelineInitializer>();
      resolver.AddDependency(typeof(IGenerateCallGraphs), callGraphGeneratorType,DependencyLifetime.Transient);

      if (callGraphGeneratorType != null)
      {
        resolver.AddDependency(typeof(IGenerateCallGraphs), callGraphGeneratorType,
          DependencyLifetime.Singleton);
      }

      foreach (var type in contributorTypes)
        resolver.AddDependency(typeof(IPipelineContributor), type, DependencyLifetime.Singleton);

      var runner = resolver.Resolve<IPipelineInitializer>();


      var opt = new StartupProperties {OpenRasta = {Pipeline = {Validate = true}}};
      options?.Invoke(opt);
      return runner.Initialize(opt);
    }
  }
}