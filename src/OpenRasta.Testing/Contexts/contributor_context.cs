using OpenRasta.Pipeline;

namespace OpenRasta.Testing.Contexts
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
            Result.LegacyShouldBe(continuation);
        }
    }
}