using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Pipeline.CallGraph
{
  class NotificationBuilder : IPipelineExecutionOrder, IPipelineExecutionOrderAnd
  {
    readonly ContributorInvocation _invocation;
    readonly IList<IPipelineContributor> _contributors;

    public NotificationBuilder(IEnumerable<IPipelineContributor> contributors, ContributorInvocation invocation)
    {
      if (contributors == null) throw new ArgumentNullException(nameof(contributors));
      _invocation = invocation;
      _contributors = contributors.ToList();
    }
    
    public IPipelineExecutionOrder And => this;
    
    public IPipelineExecutionOrderAnd After(Type contributorType)
    {
      VerifyContributorIsRegistered(contributorType);
      _invocation.AfterTypes.Add(contributorType);
      return this;
    }

    public IPipelineExecutionOrderAnd Before(Type contributorType)
    {
      VerifyContributorIsRegistered(contributorType);
      _invocation.BeforeTypes.Add(contributorType);
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