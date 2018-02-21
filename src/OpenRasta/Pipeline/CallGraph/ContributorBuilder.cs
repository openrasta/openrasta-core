using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
  internal class ContributorBuilder : IPipeline
  {
    static Type[] _knownStages =
    {
        typeof(KnownStages.IBegin),
        typeof(KnownStages.IAuthentication),
        typeof(KnownStages.IUriMatching),
        typeof(KnownStages.IHandlerSelection),
        typeof(KnownStages.IOperationCreation),
        typeof(KnownStages.IOperationFiltering),
        typeof(KnownStages.ICodecRequestSelection),
        typeof(KnownStages.IRequestDecoding),
        typeof(KnownStages.IOperationExecution),
        typeof(KnownStages.IOperationResultInvocation),
        typeof(KnownStages.ICodecResponseSelection),
        typeof(KnownStages.IResponseCoding),
        typeof(KnownStages.IEnd)
    };

    IList<IPipelineContributor> _contributors;
    List<ContributorInvocation> _invocations = new List<ContributorInvocation>();
    IPipelineContributor _contributor;
    IPipelineContributor[] _knownContributors;

    public IEnumerable<ContributorInvocation> Build(IEnumerable<IPipelineContributor> contributors)
    {
      _contributors = contributors.ToList();
      _knownContributors =
          (from stage in _knownStages
              let contributor = _contributors.FirstOrDefault(stage.IsInstanceOfType)
              where contributor != null
              select contributor).ToArray();
      return _contributors.SelectMany(BuildContributor).ToList();
    }

    IEnumerable<ContributorInvocation> BuildContributor(IPipelineContributor contributor)
    {
      _invocations = new List<ContributorInvocation>();
      _contributor = contributor;
      contributor.Initialize(this);
      if (_invocations.Count == 0)
        _invocations.Add(new ContributorInvocation(contributor, Middleware.IdentitySingleTap));
      CompleteKnownContributorDependencies(contributor);
      return _invocations;
    }

    void CompleteKnownContributorDependencies(IPipelineContributor contributor)
    {
      var index = Array.IndexOf(_knownContributors, contributor);

      if (index == -1) return;
      foreach (var invocation in _invocations)
      {
        if (index > 0 && invocation.AfterTypes.Any() == false)
          invocation.AfterTypes.Add(_knownContributors[index - 1].GetType());
        if (index + 1 < _knownContributors.Length && invocation.BeforeTypes.Any() == false)
          invocation.BeforeTypes.Add(_knownContributors[index + 1].GetType());
      }
    }

    IPipelineExecutionOrder IPipelineBuilder.NotifyAsync(Func<ICommunicationContext, Task<PipelineContinuation>> action)
    {
      var invocation = new ContributorInvocation(_contributor, action);
      _invocations.Add(invocation);
      return new NotificationBuilder(_contributors, invocation);
    }

    bool IPipeline.IsInitialized => throw new NotSupportedException();
    IList<IPipelineContributor> IPipeline.Contributors => _contributors;
    IEnumerable<ContributorCall> IPipeline.CallGraph { get; } = Enumerable.Empty<ContributorCall>();

    void IPipeline.Initialize() => throw new NotSupportedException();

    IPipelineExecutionOrder IPipeline.Notify(Func<ICommunicationContext, PipelineContinuation> notification)
    {
      return ((IPipelineBuilder)this).NotifyAsync(ctx => Task.FromResult(notification(ctx)));
    }

    void IPipeline.Run(ICommunicationContext context) => throw new NotSupportedException();
  }
}