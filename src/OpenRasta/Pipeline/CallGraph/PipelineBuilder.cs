using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
  internal class PipelineBuilder : IPipeline
  {
    public bool IsInitialized { get; } = false;
    public IList<IPipelineContributor> Contributors { get; }
    public IEnumerable<ContributorCall> CallGraph { get; } = Enumerable.Empty<ContributorCall>();
    public IList<Notification> ContributorRegistrations { get; private set; } = new List<Notification>();

    public PipelineBuilder(IEnumerable<IPipelineContributor> contributors)
    {
      Contributors = contributors.ToList();
    }
    public void Initialize()
    {
    }

    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, PipelineContinuation> method)
    {
      var notification = new Notification(method, Contributors);
      ContributorRegistrations.Add(notification);
      return notification;
    }
    [Obsolete]
    public void Run(ICommunicationContext context)
    {
      throw new NotImplementedException("No one should be calling Run on this");
    }
  }
}
