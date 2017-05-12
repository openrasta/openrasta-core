using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Concordia;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
    internal class PipelineBuilder : IPipeline, IPipelineBuilder
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
            var notification = new Notification(_ => Task.FromResult(method(_)), Contributors);
            ContributorRegistrations.Add(notification);
            return notification;
        }

        [Obsolete]
        public void Run(ICommunicationContext context)
        {
            throw new NotImplementedException("Backward compatibility implementation, should never be called.");
        }

        public IPipelineExecutionOrder NotifyAsync(Func<ICommunicationContext, Task<PipelineContinuation>> action)
        {
            var notification = new Notification(action, Contributors);
            ContributorRegistrations.Add(notification);
            return notification;
        }

        public IPipelineExecutionOrder Notify(Func<ICommunicationContext, Task> action)
        {
            var notification = new Notification(
                async env =>
                {
                    await action(env);
                    return PipelineContinuation.Continue;
                }, Contributors);
            ContributorRegistrations.Add(notification);
            return notification;
        }

        [Obsolete]
        public void Initialize(StartupProperties props)
        {
            throw new NotImplementedException("Backward compatibility implementation, should never be called.");
        }
    }
}