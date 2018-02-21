using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
    public class EndContributor : KnownStages.IEnd
    {
        public void Initialize(IPipeline pipelineRunner)
        {
          pipelineRunner.Notify(context => PipelineContinuation.Finished);
        }
    }
}