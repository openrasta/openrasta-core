using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
    internal class Notification : IPipelineExecutionOrder, IPipelineExecutionOrderAnd
    {
        readonly IList<IPipelineContributor> _contributors;

        public Notification(
            Func<ICommunicationContext, Task<PipelineContinuation>> action,
            IEnumerable<IPipelineContributor> contributors)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (contributors == null) throw new ArgumentNullException(nameof(contributors));
            _contributors = contributors.ToList();
            Target = action;
        }


        public ICollection<Type> AfterTypes { get; } = new List<Type>();

        public IPipelineExecutionOrder And => this;

        public ICollection<Type> BeforeTypes { get; } = new List<Type>();

        public string Description => Target?.Target?.GetType().Name;

        public Func<ICommunicationContext, Task<PipelineContinuation>> Target { get; }

        public IPipelineExecutionOrderAnd After(Type contributorType)
        {
            VerifyContributorIsRegistered(contributorType);
            AfterTypes.Add(contributorType);
            return this;
        }

        public IPipelineExecutionOrderAnd Before(Type contributorType)
        {
            VerifyContributorIsRegistered(contributorType);
            BeforeTypes.Add(contributorType);
            return this;
        }

        void VerifyContributorIsRegistered(Type contributorType)
        {
            if (!GetContributorsOfType(contributorType).Any())
                throw new DependentContributorMissingException(contributorType);
        }

        IEnumerable<IPipelineContributor> GetContributorsOfType(Type contributorType)
        {
            return from contributor in _contributors
                where contributorType.IsInstanceOfType(contributor)
                select contributor;
        }
    }
}