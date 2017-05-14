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
        var orderedCallGraph = _callGrapher
          .GenerateCallGraph(_contributors)
          .ToList();
        var orderedContributor = orderedCallGraph.Select(c => c.Target).ToList();
        var orderedContributorMiddleware = orderedCallGraph
          .ToDetailedMiddleware(startup.OpenRasta.Pipeline.ContributorTrailers)
          .ToList();

        var orderedMiddleware = defaults
          .Concat(orderedContributorMiddleware.Select(c=>c.Item1));

        foreach (var defaultMiddleware in defaults)
          LogMiddleware(defaultMiddleware);
        foreach (var contributor in orderedContributorMiddleware)
        {
          LogMiddleware(contributor.Item1);

          if (contributor.Item2 != null)
            Log.WriteInfo($"Initialized contributor {contributor.Item2.Target.GetType().Name}");
        }

        return new PipelineAsync(orderedContributor, orderedMiddleware.Compose(), orderedCallGraph);
      }
    }

    static void LogMiddleware(IPipelineMiddlewareFactory factory)
    {
      Log.WriteInfo($"Initialized middleware factory {factory.GetType().Name}");
    }
  }
}