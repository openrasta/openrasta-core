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

    public static IPipelineMiddleware ToTwoPhasedMiddleware<TResponseType>(this IEnumerable<ContributorCall> callGraph)
    {
      var all = callGraph.Reverse().ToList();

      var responsePipeline = all
        .TakeWhile(_ => _.Target is TResponseType == false)
        .Select(CreateResponseMiddleware)
        .BuildPipeline();

      var requerstPipeline = all
        .SkipWhile(_ => _.Target is TResponseType == false)
        .Select(CreateRequestMiddleware)
        .BuildPipeline();

      return new TwoPhasedMiddleware(
        requerstPipeline,
        responsePipeline,
        new CatastrophicFailureMiddleware(),
        new CleanupMiddleware());
    }


    static IPipelineMiddlewareFactory CreateResponseMiddleware(ContributorCall contrib)
    {
      return new ResponseMiddleware(contrib.Action);
    }
    static KeyValuePair<string,IPipelineMiddlewareFactory> CreateNamedRequestMiddleware(ContributorCall contrib)
    {
      return new KeyValuePair<string, IPipelineMiddlewareFactory>(
        contrib.Target.GetType().Name,
        new RequestMiddleware(contrib.Action));
    }
    static IPipelineMiddlewareFactory CreateRequestMiddleware(ContributorCall contrib)
    {
      return new RequestMiddleware(contrib.Action);
    }
  }
}