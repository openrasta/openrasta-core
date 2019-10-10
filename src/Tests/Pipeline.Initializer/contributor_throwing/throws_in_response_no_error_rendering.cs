using System;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline.Contributors;
using Shouldly;
using Tests.Pipeline.Initializer.Infrastructure;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  public class throws_in_response_no_error_rendering : initializer_context
  {
    public Type[] Contributors { get; set; }
    protected InMemoryCommunicationContext Context { get; } = new InMemoryCommunicationContext();

    public throws_in_response_no_error_rendering()
    {
      Contributors = new[]
      {
        typeof(PreExecutingContributor),
        typeof(RequestPhaseContributor),
        typeof(ResponsePhaseContributor),
        typeof(ContributorThrowingAfter<ResponsePhaseContributor>)
      };
    }


//    [Theory]
//    [InlineData(typeof(WeightedCallGraphGenerator))]
//    [InlineData(typeof(TopologicalSortCallGraphGenerator))]
    public async Task alway_call_post_execute_contributor(Type callGraphGeneratorType)
    {
      await Should.ThrowAsync<InvalidOperationException>(() => RunPipeline(callGraphGeneratorType));

      Context.PipelineData.ContainsKey(nameof(PostExecuteMarkerContributor)).ShouldBeTrue();
    }

    protected async Task RunPipeline(Type callGraphGeneratorType)
    {
      var pipeline = CreatePipeline(callGraphGeneratorType, Contributors.Concat(new[]
      {
        typeof(RequestResponseDisposer),
        typeof(PostExecuteMarkerContributor)
      }).ToArray(), opt =>
      {
        opt.OpenRasta.Pipeline.Validate = false;
        opt.OpenRasta.Errors.HandleAllExceptions = false;
        opt.OpenRasta.Errors.HandleCatastrophicExceptions = false;
      });
      await pipeline.RunAsync(Context);
    }
  }
}