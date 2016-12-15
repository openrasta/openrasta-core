using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
    internal class Notification : IPipelineExecutionOrder, IPipelineExecutionOrderAnd
    {
        readonly ICollection<Type> _after = new List<Type>();
        readonly ICollection<Type> _before = new List<Type>();
        readonly PipelineRunner _runner;

        public Notification(PipelineRunner runner, Func<ICommunicationContext, PipelineContinuation> action)
        {
            _runner = runner;
            Target = action;
        }

        public ICollection<Type> AfterTypes
        {
            get { return _after; }
        }

        public IPipelineExecutionOrder And
        {
            get { return this; }
        }

        public ICollection<Type> BeforeTypes
        {
            get { return _before; }
        }

        public string Description
        {
            get { return Target != null && Target.Target != null ? Target.Target.GetType().Name : null; }
        }

        public Func<ICommunicationContext, PipelineContinuation> Target { get; private set; }

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
            return from contributor in _runner.Contributors
                   where contributorType.IsInstanceOfType(contributor)
                   select contributor;
        }
    }

    public class DependentContributorMissingException : Exception
    {
        public IEnumerable<Type> ContributorTypes { get; set; }

        public DependentContributorMissingException(params Type[] contributorTypes)
            : this((IEnumerable<Type>)contributorTypes)

        {
        }

        public DependentContributorMissingException(IEnumerable<Type> contributorTypes)
            : base("Dependent contributor(s) missing, ensure they are added to the pipeline: "
            + string.Join(", ", contributorTypes.Select(c=>c.Name)))
        {
            ContributorTypes = contributorTypes;
        }
    }
}