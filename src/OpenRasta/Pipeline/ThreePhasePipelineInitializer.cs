using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Concordia;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Pipeline.CallGraph;

namespace OpenRasta.Pipeline
{
  public class ThreePhasePipelineInitializer : IPipelineInitializer
  {
    static ILogger Log { get; } = TraceSourceLogger.Instance;

    public IPipelineAsync Initialize(StartupProperties startup)
    {
      var factories = startup.OpenRasta.Factories;

      var contributors = factories.Contributors().ToList();
      
      if (startup.OpenRasta.Pipeline.Validate)
        contributors.VerifyKnownStagesRegistered();

      var defaults = new List<IPipelineMiddlewareFactory>()
      {
        new CleanupMiddleware()
      };
      if (startup.OpenRasta.Errors.HandleCatastrophicExceptions)
      {
        defaults.Add(new CatastrophicFailureMiddleware());
      }
      Func<
        IGenerateCallGraphs,
        IEnumerable<IPipelineMiddlewareFactory>,
        IEnumerable<IPipelineContributor>,
        StartupProperties,
        IEnumerable<(IPipelineMiddlewareFactory middleware, ContributorCall contributor)>
      > builder;

      if (startup.OpenRasta.Diagnostics.TracePipelineExecution)
        builder = LogBuild;
      else
        builder = Build;

      var pipeline = builder(
          factories.CallGraphGenerator(),
          defaults,
          contributors,
          startup)
        .ToList();

      return new PipelineAsync(
        pipeline.Select(c => c.middleware).Compose(),
        pipeline.Select(c => c.contributor?.Target).Where(c => c != null).ToList(),
        pipeline.Select(c => c.contributor).ToList(),
        pipeline.Select(c => c.middleware).ToList());
    }

    static IEnumerable<(IPipelineMiddlewareFactory, ContributorCall)> Build(IGenerateCallGraphs callGraphGenerator, IEnumerable<IPipelineMiddlewareFactory> defaults, IEnumerable<IPipelineContributor> contributors, StartupProperties startupProperties)
    {
      return Defaults(defaults).Concat(Contributors(callGraphGenerator, contributors, startupProperties));
    }

    static IEnumerable<ValueTuple<IPipelineMiddlewareFactory, ContributorCall>> Contributors(IGenerateCallGraphs callGraphGenerator, IEnumerable<IPipelineContributor> contributors,
      StartupProperties startupProperties)
    {
      return callGraphGenerator.GenerateCallGraph(contributors).ToDetailedMiddleware(startupProperties);
    }

    static IEnumerable<ValueTuple<IPipelineMiddlewareFactory, ContributorCall>> Defaults(IEnumerable<IPipelineMiddlewareFactory> defaults)
    {
      foreach (var factory in defaults) yield return (factory, null);
    }

    IEnumerable<(IPipelineMiddlewareFactory middleware, ContributorCall contributor)> LogBuild(IGenerateCallGraphs callGraphGenerator, IEnumerable<IPipelineMiddlewareFactory> defaults, IEnumerable<IPipelineContributor> contributors, StartupProperties startupProperties)
    {
      var loggingFactory = new LoggingMiddlewareFactory();

      using (Log.Operation(this, $"Initializing the pipeline. (using {callGraphGenerator.GetType()})"))
      {
        foreach (var result in Build(callGraphGenerator, defaults, contributors, startupProperties))
        {
          yield return (loggingFactory, null);
          yield return LogBuildEntry(result);
        }
      }
    }

    (IPipelineMiddlewareFactory middleware, ContributorCall contributor) LogBuildEntry(
      (IPipelineMiddlewareFactory middleware, ContributorCall call) result)
    {
      var middleware = $"middleware {result.middleware.GetType().Name}";

      Log.WriteInfo(result.call != null
        ? $"Initialized contributor {result.call.ContributorTypeName} ({middleware})"
        : $"Initialized {middleware}");

      return result;
    }
  }
}