using OpenRasta.Pipeline;
using Shouldly;

namespace OpenRasta.Tests.Unit.Infrastructure
{
    public abstract class contributor_context<T> : openrasta_context
        where T : class, IPipelineContributor
    {
        public void given_contributor()
        {
            given_pipeline_contributor<T>();
        }

        protected void then_contributor_returns(PipelineContinuation continuation)
        {
          Result.ShouldBe(continuation);
          //return valueToAnalyse;
        }
    }
}