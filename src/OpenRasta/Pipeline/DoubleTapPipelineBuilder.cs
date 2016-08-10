using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Pipeline.Contributors;

namespace OpenRasta.Pipeline
{
  public static class DoubleTapPipelineBuilder
  {
    public static IPipelineComponent Build(IGenerateCallGraphs graphs, IEnumerable<IPipelineContributor> contributors)
    {
      var all = graphs.GenerateCallGraph(contributors).Reverse().ToList();

      var renderPipeline = all
        .TakeWhile(_ => _.Target is KnownStages.IOperationResultInvocation == false)
        .Select(CreateRenderComponent).BuildPipeline();

      var executePipeline = all
        .SkipWhile(_ => _.Target is KnownStages.IOperationResultInvocation == false)
        .Select(CreatePrerenderComponent)
        .BuildPipeline();

      return new FullPipelineComponent(
        executePipeline,
        renderPipeline,
        new CatastrophicFailureComponent(),
        new CleanupPipelineComponent());
    }



    static IPipelineMiddleware CreateRenderComponent(ContributorCall contrib)
    {
      return new RenderComponent(contrib.Action);

    }

    static IPipelineMiddleware CreatePrerenderComponent(ContributorCall contrib)
    {
      return new ExecutionPipelineComponent(contrib.Action);
    }
  }
}
