using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
    public class ResponseMiddleware : AbstractContributorMiddleware
    {
        public ResponseMiddleware(Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor) : base(singleTapContributor)
        {
        }

        public override async Task Invoke(ICommunicationContext env)
        {
            env.PipelineData.PipelineStage.CurrentState
                = await InvokeSingleTap(env);
            await Next.Invoke(env);
        }
    }
}
