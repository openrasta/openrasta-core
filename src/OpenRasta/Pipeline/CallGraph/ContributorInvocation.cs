using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
  class ContributorInvocation
  {
    public ContributorInvocation(IPipelineContributor owner, Func<ICommunicationContext, Task<PipelineContinuation>> action)
    {
      Owner = owner;
      Target = action ?? throw new ArgumentNullException(nameof(action));
    }
    public string Description => Target?.Target?.GetType().Name;

    public IPipelineContributor Owner { get; }
    public Func<ICommunicationContext, Task<PipelineContinuation>> Target { get; }
    
    public ICollection<Type> BeforeTypes { get; } = new List<Type>();
    public ICollection<Type> AfterTypes { get; } = new List<Type>();
    public override string ToString()
    {
      return Owner.GetType().ToString();
    }
  }
}