using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  [Obsolete("Runner is no longer supported")]
  public class PipelineRunner : IPipeline
  {
    // ReSharper disable once UnusedParameter.Local - Compatibility Shim
    public PipelineRunner()
    {
      throw new NotImplementedException("Runner is no longer supported");
    }

    public IList<IPipelineContributor> Contributors => null;

    public bool IsInitialized => false;

    public IEnumerable<ContributorCall> CallGraph { get; } = Enumerable.Empty<ContributorCall>();

    public void Initialize()
    {
      throw new NotImplementedException("Runner is no longer supported");
    }

    [Obsolete("Don't do it this will deadlock.")]
    public void Run(ICommunicationContext context)
    {
      throw new NotImplementedException("Runner is no longer supported");
    }

    public IPipelineExecutionOrder NotifyAsync(Func<ICommunicationContext, Task<PipelineContinuation>> action)
    {
      throw new NotImplementedException("Shouldn't be called here ever.");
    }

    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, PipelineContinuation> action)
    {
      throw new NotImplementedException("This code has moved. Try using a CompatibilityContributorInitializer instad.");
    }
  }
}