using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using OpenRasta.Pipeline.Contributors;

namespace OpenRasta.Pipeline
{
  public static class DoubleTapPipelineExtensions
  {
    public static IPipelineMiddleware ToDoubleTapMiddleware<TResponseType>(this IEnumerable<ContributorCall> callGraph)
    {
      var all = callGraph.Reverse().ToList();

      var responsePipeline = all
        .TakeWhile(_ => _.Target is TResponseType == false)
        .Select(CreateResponseMiddleware).BuildPipeline();

      var requerstPipeline = all
        .SkipWhile(_ => _.Target is TResponseType == false)
        .Select(CreateRequestMiddleware)
        .BuildPipeline();

      return new DoubleTapMiddleware(
        requerstPipeline,
        responsePipeline,
        new CatastrophicFailureMiddleware(),
        new CleanupMiddleware());
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
