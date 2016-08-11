using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
    public class RenderComponent : AbstractPipelineComponent
    {
        public RenderComponent(Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor) : base(singleTapContributor)
        {
        }

        public override async Task Invoke(ICommunicationContext env)
        {
            var currentState
              = env.PipelineData.PipelineStage.CurrentState
                = await InvokeSingleTap(env);
            await Next.Invoke(env);
        }
    }
}
