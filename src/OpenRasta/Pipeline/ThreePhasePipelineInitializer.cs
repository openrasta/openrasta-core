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
    readonly IEnumerable<IPipelineContributor> _contributors;
    readonly IGenerateCallGraphs _callGrapher;
    static ILogger Log { get; } = new TraceSourceLogger();


    public ThreePhasePipelineInitializer(IDependencyResolver resolver)
      : this(resolver.ResolveAll<IPipelineContributor>(),
        new CallGraphGeneratorFactory(resolver).GetCallGraphGenerator())
    {
    }

    ThreePhasePipelineInitializer(
      IEnumerable<IPipelineContributor> contributors,
      IGenerateCallGraphs callGrapher)
    {
      _contributors = contributors;
      _callGrapher = callGrapher;
    }

    public IPipelineAsync Initialize(StartupProperties startup)
    {
      if (startup.OpenRasta.Pipeline.Validate)
        _contributors.VerifyKnownStagesRegistered();

      using (Log.Operation(this, "Initializing the pipeline."))
      {
        var defaults = new List<IPipelineMiddlewareFactory>();
        if (startup.OpenRasta.Errors.HandleCatastrophicExceptions)
        {
          defaults.Add(new CatastrophicFailureMiddleware());
        }
        var pipeline = LogBuild(
          _callGrapher,
          defaults,
          _contributors).ToList();

        return new PipelineAsync(
          pipeline.Select(c=>c.contributor?.Target).Where(c=>c!=null).ToList(),
          pipeline.Select(c=>c.middleware).Compose(),
          pipeline.Select(c=>c.contributor).ToList());
      }

    }

    static IEnumerable<(IPipelineMiddlewareFactory, ContributorCall)> Build(
      IGenerateCallGraphs callGraphGenerator,
      IEnumerable<IPipelineMiddlewareFactory> defaults,
      IEnumerable<IPipelineContributor> contributors)
    {
      foreach (var factory in defaults)
      {
        yield return (factory, null);
      }
      foreach (var contributor in
        callGraphGenerator.GenerateCallGraph(contributors)
          .ToDetailedMiddleware())
      {
        yield return contributor;
      }
    }

    IEnumerable<(IPipelineMiddlewareFactory middleware, ContributorCall contributor)> LogBuild(
      IGenerateCallGraphs callGraphGenerator,
      IEnumerable<IPipelineMiddlewareFactory> defaults,
      IEnumerable<IPipelineContributor> contributors)
    {
      using (Log.Operation(this, $"Initializing the pipeline. (using {callGraphGenerator.GetType()})"))
      {
        foreach (var result in Build(callGraphGenerator, defaults, contributors))
          yield return LogBuildEntry(result);
      }
    }

    (IPipelineMiddlewareFactory middleware, ContributorCall contributor) LogBuildEntry(
      (IPipelineMiddlewareFactory middleware, ContributorCall call) result)
    {
      using (Log.Operation(this, $"Initializing middleware {result.middleware.GetType().Name}"))
      {
        if (result.call != null)
          Log.WriteInfo($"Initialized contributor {result.call.ContributorTypeName}");
        return result;
      }
    }
  }
}