using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public interface IPipelineBuilder
  {
    IPipelineExecutionOrder Notify(Func<ICommunicationContext, Task<PipelineContinuation>> action);
    IPipelineExecutionOrder Notify(Func<ICommunicationContext, Task> action);

  }
}