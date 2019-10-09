using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
  public class RequestResponseDisposer : KnownStages.IEnd
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(FinishRequest);
    }

    PipelineContinuation FinishRequest(ICommunicationContext context)
    {
      context.Request.Entity?.Dispose();
      context.Response.Entity?.Dispose();
      return PipelineContinuation.Continue;
    }
  }
}