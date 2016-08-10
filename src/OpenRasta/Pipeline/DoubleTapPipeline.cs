using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Collections;
using OpenRasta.DI;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class DoubleTapPipeline
  {
    FullPipelineComponent _pipeline;

    void Build(Func<IGenerateCallGraphs> graphs, IEnumerable<IPipelineContributor> contributors)
    {
      var all = graphs().GenerateCallGraph(contributors).Reverse().ToList();

      var renderPipeline = all
        .TakeWhile(_ => _.Target is KnownStages.IOperationResultInvocation == false)
        .Select(CreateRenderComponent).BuildPipeline();

      var executePipeline = all
        .SkipWhile(_ => _.Target is KnownStages.IOperationResultInvocation == false)
        .Select(CreatePrerenderComponent)
        .BuildPipeline();

      _pipeline = new FullPipelineComponent(
        executePipeline,
        renderPipeline,
        new CatastrophicFailureComponent(),
        new CleanupPipelineComponent());
    }

    Task Run(ICommunicationContext env)
    {
      return _pipeline.Invoke(env);
    }


    static IPipelineMiddleware CreateRenderComponent(ContributorCall contrib)
    {
      return new RenderComponent(env => Task.FromResult(contrib.Action(env)));

    }

    static IPipelineMiddleware CreatePrerenderComponent(ContributorCall contrib)
    {
      return new ExecutionPipelineComponent(env => Task.FromResult(contrib.Action(env)));
    }
  }
}
