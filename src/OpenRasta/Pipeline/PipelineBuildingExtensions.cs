using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Pipeline
{
  public static class PipelineBuildingExtensions
  {
    static readonly List<Type> MiddelwareStageTypes = new List<Type>
    {
      typeof(KnownStages.IBegin),
      typeof(KnownStages.IUriMatching),
      typeof(KnownStages.IAuthentication),
      typeof(KnownStages.IRequestDecoding),
      typeof(KnownStages.IOperationExecution),
      typeof(KnownStages.IResponseCoding),
      typeof(KnownStages.IEnd)
    };
//    public static IEnumerable<KeyValuePair<Type,IEnumerable<IPipelineMiddlewareFactory>>> ToMiddlewareStages(this IEnumerable<ContributorCall> callStack)
//    {
//      var factories = new List<IPipelineMiddlewareFactory>();
//      var responsePhase = false;
//
//      foreach (var contrib in callStack)
//      {
//        factories.Add(responsePhase
//          ? CreateResponseMiddleware(contrib)
//          : CreateRequestMiddleware(contrib));
//        var stageType = MiddelwareStageTypes.FirstOrDefault(knownType => knownType.IsInstanceOfType(contrib.Target));
//        if (stageType != null)
//        {
//          yield return new KeyValuePair<Type, IEnumerable<IPipelineMiddlewareFactory>>(
//            stageType,
//            new List<IPipelineMiddlewareFactory>(factories));
//          factories.Clear();
//        }
//        responsePhase = responsePhase || contrib.Target is KnownStages.IOperationExecution;
//      }
//
//      if (factories.Any())
//        throw new InvalidOperationException("Somehow there is a contributor after IEnd. That's absurd.");
//    }

//    public static IEnumerable<KeyValuePair<string,IPipelineMiddlewareFactory>> ToNamedMiddleware<TResponseType>(this IEnumerable<ContributorCall> callGraph)
//    {
//      foreach (var call in callGraph)
//      {
//        yield return new KeyValuePair<string, IPipelineMiddlewareFactory>(
//          call.Target.GetType().Name
//        );
//      }
//    }

    public static IEnumerable<IPipelineMiddlewareFactory> ToMiddleware(
      this IEnumerable<ContributorCall> callGraph,
      IDictionary<Func<ContributorCall, bool>, Func<IPipelineMiddlewareFactory, IPipelineMiddlewareFactory>>
        interceptors)
    {
      Func<ContributorCall, IPipelineMiddlewareFactory> converter = CreatePreExecuteMiddleware;

      foreach (var contributorCall in callGraph)
      {
        var middleware = AttemptInterception(interceptors,contributorCall,converter(contributorCall));

        yield return middleware;

        if (contributorCall.Target is KnownStages.IUriMatching)
          converter = CreateRequestMiddleware;
        if (contributorCall.Target is KnownStages.IOperationResultInvocation)
          converter = CreateResponseMiddleware;

      }
    }
    public static IPipelineMiddleware ToThreePhasedMiddleware(this IEnumerable<ContributorCall> callGraph, IDictionary<Func<ContributorCall, bool>, Func<IPipelineMiddlewareFactory, IPipelineMiddlewareFactory>> interceptors)
    {
      var all = callGraph.Reverse().ToList();

      var responsePipeline = (
          from contrib in all.TakeWhile(_ => _.Target is KnownStages.IOperationResultInvocation == false)
          let middleware = CreateResponseMiddleware(contrib)
          select AttemptInterception(interceptors,contrib,middleware)
      ).BuildPipeline();

      var requestPipeline = all
        .SkipWhile(_ => _.Target is KnownStages.IOperationResultInvocation == false)
        .Select(CreateRequestMiddleware)
        .BuildPipeline();

      var preExecutePipeline = all
        .SkipWhile(_ => _.Target is KnownStages.IUriMatching == false)
        .Select(CreatePreExecuteMiddleware)
        .BuildPipeline();

      return new ThreePhasedMiddleware(
        preExecutePipeline,
        requestPipeline,
        responsePipeline,
        new CatastrophicFailureMiddleware(),
        new CleanupMiddleware());
    }

    static IPipelineMiddlewareFactory AttemptInterception(IDictionary<Func<ContributorCall, bool>, Func<IPipelineMiddlewareFactory, IPipelineMiddlewareFactory>> interceptors, ContributorCall contrib, IPipelineMiddlewareFactory middleware)
    {
      return interceptors
        .Where(i => i.Key(contrib))
        .Select(i => i.Value)
        .Aggregate(middleware, (factory, interceptor) => interceptor(factory));
    }

    static IPipelineMiddlewareFactory CreatePreExecuteMiddleware(ContributorCall contrib)
    {
      return new PreExecuteMiddleware(contrib.Action);
    }

    static IPipelineMiddlewareFactory CreateResponseMiddleware(ContributorCall contrib)
    {
      return new ResponseMiddleware(contrib.Action);
    }

    static IPipelineMiddlewareFactory CreateRequestMiddleware(ContributorCall contrib)
    {
      return new RequestMiddleware(contrib.Action);
    }
  }
}