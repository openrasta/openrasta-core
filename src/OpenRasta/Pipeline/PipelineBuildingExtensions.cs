using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Concordia;

namespace OpenRasta.Pipeline
{
  public static class PipelineBuildingExtensions
  {
    public static IEnumerable<IPipelineMiddlewareFactory> ToMiddleware(
      this IEnumerable<ContributorCall> callGraph,
      StartupProperties startupProperties = null)
    {
      return ToDetailedMiddleware(callGraph, startupProperties).Select(m => m.Item1).ToList();
    }

    public static IEnumerable<(IPipelineMiddlewareFactory, ContributorCall)> ToDetailedMiddleware(
      this IEnumerable<ContributorCall> callGraph,
      StartupProperties startupProperties = null)
    {
      Func<ContributorCall, IPipelineMiddlewareFactory> converter = CreatePreExecuteMiddleware;


      foreach (var contributorCall in callGraph)
      {
        if (contributorCall.Target is KnownStages.IOperationResultInvocation)
        {
          converter = CreateResponseMiddleware;
          if (startupProperties?.OpenRasta.Errors.HandleAllExceptions == true)
            yield return (new ResponseRetryMiddleware(), null);
        }
        
        if (contributorCall.Target is KnownStages.IUriMatching)
          converter = CreateRequestMiddleware;
        
        var middleware = converter(contributorCall);
        yield return (middleware, contributorCall);
        if (startupProperties?.OpenRasta.Pipeline.ContributorTrailers != null)
          foreach (var followups in startupProperties.OpenRasta.Pipeline.ContributorTrailers
            .Where(pair => pair.Key(contributorCall))
            .Select(pair => pair.Value))
            yield return (followups(), null);

      }
    }

    static IPipelineMiddlewareFactory CreatePreExecuteMiddleware(ContributorCall contrib)
    {
      return new PreExecuteMiddleware(contrib);
    }

    static IPipelineMiddlewareFactory CreateResponseMiddleware(ContributorCall contrib)
    {
      return new ResponseMiddleware(contrib);
    }

    static IPipelineMiddlewareFactory CreateRequestMiddleware(ContributorCall contrib)
    {
      return new RequestMiddleware(contrib);
    }
  }
}