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
      StartupProperties startupProperties = null,
      IDictionary<Func<ContributorCall, bool>, Func<IPipelineMiddlewareFactory>>
        interceptors = null)
    {
      return ToDetailedMiddleware(callGraph, startupProperties, interceptors).Select(m => m.Item1).ToList();
    }

    public static IEnumerable<(IPipelineMiddlewareFactory, ContributorCall)> ToDetailedMiddleware(
      this IEnumerable<ContributorCall> callGraph, StartupProperties startupProperties = null,
      IDictionary<Func<ContributorCall, bool>, Func<IPipelineMiddlewareFactory>> interceptors = null)
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
        var middleware = converter(contributorCall);
        yield return (middleware, contributorCall);
        if (interceptors != null)
          foreach (var followups in interceptors
            .Where(pair => pair.Key(contributorCall))
            .Select(pair => pair.Value))
            yield return (followups(), null);

        if (contributorCall.Target is KnownStages.IUriMatching)
          converter = CreateRequestMiddleware;
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