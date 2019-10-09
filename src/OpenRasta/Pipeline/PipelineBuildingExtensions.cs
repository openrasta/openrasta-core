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
      Func<ContributorCall,StartupProperties, IPipelineMiddlewareFactory> converter = CreatePreExecuteMiddleware;


      foreach (var contributorCall in callGraph)
      {
        // change the type of middleware to create at key known stages
        switch (contributorCall.Target)
        {
          case KnownStages.IUriMatching _:
            converter = CreateRequestMiddleware;
            break;
          case KnownStages.IOperationResultInvocation _:
          {
            converter = CreateResponseMiddleware;
            if (startupProperties?.OpenRasta.Errors.HandleAllExceptions == true)
              yield return (new ResponseRetryMiddleware(), null);
            break;
          }
          case KnownStages.IEnd _:
            converter = CreatePostExecuteMiddleware;
            break;
        }

        

        var middleware = converter(contributorCall, startupProperties);
        yield return (middleware, contributorCall);
        
        if (startupProperties?.OpenRasta.Pipeline.ContributorTrailers == null) continue;
        
        foreach (var followups in startupProperties.OpenRasta.Pipeline.ContributorTrailers
          .Where(pair => pair.Key(contributorCall))
          .Select(pair => pair.Value))
          yield return (followups(), null);

      }
    }

    static IPipelineMiddlewareFactory CreatePreExecuteMiddleware(ContributorCall contrib, StartupProperties props)
    {
      return new PreExecuteMiddleware(contrib);
    }

    static IPipelineMiddlewareFactory CreatePostExecuteMiddleware(ContributorCall contrib, StartupProperties props)
    {
      return new PostExecuteMiddleware(contrib);
    }

    static IPipelineMiddlewareFactory CreateResponseMiddleware(ContributorCall contrib, StartupProperties props)
    {
      return new ResponseMiddleware(contrib);
    }

    static IPipelineMiddlewareFactory CreateRequestMiddleware(ContributorCall contrib, StartupProperties props)
    {
      return new RequestMiddleware(contrib, props?.OpenRasta.Errors.HandleAllExceptions ?? true);
    }
  }
}