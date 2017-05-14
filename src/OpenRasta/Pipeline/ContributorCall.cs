using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class ContributorCall
  {
    IPipelineContributor _target;

    public ContributorCall()
    {
      ContributorTypeName = "Unknown";
    }

    public ContributorCall(
      IPipelineContributor target,
      Func<ICommunicationContext, Task<PipelineContinuation>> action,
      string description)
    {
      if (target == null) throw new ArgumentNullException(nameof(target));
      if (action == null) throw new ArgumentNullException(nameof(action));
      Action = action;
      ContributorTypeName = description;
      Target = target;
    }

    public string ContributorTypeName { get; set; }

    public IPipelineContributor Target
    {
      get { return _target; }
      set
      {
        _target = value;
        if (_target != null && ContributorTypeName == null)
          ContributorTypeName = _target.GetType().Name;
      }
    }

    public Func<ICommunicationContext, Task<PipelineContinuation>> Action { get; set; }
  }
}