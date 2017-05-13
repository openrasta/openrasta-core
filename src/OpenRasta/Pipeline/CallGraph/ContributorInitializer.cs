using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
  internal class ContributorInitializer : IPipelineBuilder
  {
    public IEnumerable<IPipelineContributor> Contributors { get; }
    public IEnumerable<ContributorCall> CallGraph { get; } = Enumerable.Empty<ContributorCall>();
    public IEnumerable<Notification> ContributorRegistrations => _registrations;
    readonly List<Notification> _registrations =  new List<Notification>();

    public ContributorInitializer(IEnumerable<IPipelineContributor> contributors)
    {
      Contributors = contributors.ToList();
    }

    public IPipelineExecutionOrder NotifyAsync(Func<ICommunicationContext, Task<PipelineContinuation>> action)
    {
      var notification = new Notification(action, Contributors);
      _registrations.Add(notification);
      return notification;
    }
  }
}