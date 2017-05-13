using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Concordia;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
  internal class CompatibilityContributorInitializer : IPipeline
  {
    readonly ContributorInitializer _initializer;
    public bool IsInitialized { get; } = false;

    public IList<IPipelineContributor> Contributors => _initializer.Contributors.ToList().AsReadOnly();

    public IEnumerable<ContributorCall> CallGraph => _initializer.CallGraph;

    public IList<Notification> ContributorRegistrations => _initializer.ContributorRegistrations.ToList().AsReadOnly();

    public CompatibilityContributorInitializer(ContributorInitializer initializer)
    {
      _initializer = initializer;
    }

    public void Initialize()
    {
      throw new NotImplementedException("Backward compatibility implementation, should never be called.");
    }

    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, PipelineContinuation> method)
    {
      return _initializer.NotifyAsync(env => Task.FromResult(method(env)));
    }

    public IPipelineExecutionOrder NotifyAsync(Func<ICommunicationContext, Task<PipelineContinuation>> action)
      => _initializer.NotifyAsync(action);

    [Obsolete]
    public void Run(ICommunicationContext context)
    {
      throw new NotImplementedException("Backward compatibility implementation, should never be called.");
    }
  }
}