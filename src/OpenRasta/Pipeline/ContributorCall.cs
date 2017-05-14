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
      Action = action ?? throw new ArgumentNullException(nameof(action));
      Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public string ContributorTypeName { get; set; }

    public IPipelineContributor Target
    {
      get => _target;
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