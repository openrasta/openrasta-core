using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
    internal class Notification : IPipelineExecutionOrder, IPipelineExecutionOrderAnd
    {
      readonly IList<IPipelineContributor> _contributors;

      public Notification(Func<ICommunicationContext, PipelineContinuation> action, IEnumerable<IPipelineContributor> contributors)
        {
          _contributors = contributors.ToList();
            Target = action;
        }

        public ICollection<Type> AfterTypes { get; } = new List<Type>();

      public IPipelineExecutionOrder And => this;

      public ICollection<Type> BeforeTypes { get; } = new List<Type>();

      public string Description => Target?.Target?.GetType().Name;

      public Func<ICommunicationContext, PipelineContinuation> Target { get; }

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
                throw new ArgumentOutOfRangeException("There is no registered contributor matching type " + contributorType.FullName);
        }

        IEnumerable<IPipelineContributor> GetContributorsOfType(Type contributorType)
        {
            return from contributor in _contributors
                   where contributorType.IsInstanceOfType(contributor)
                   select contributor;
        }
    }
}
