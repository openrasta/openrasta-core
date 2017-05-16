using System;
using System.Collections;
using System.Collections.Generic;
using OpenRasta.Collections;

namespace OpenRasta.Pipeline
{
  public class PipelineStage : IEnumerable<ContributorCall>
  {
    public PipelineStage OwnerStage => null;

    public PipelineStage()
    {
      CurrentState = PipelineContinuation.Continue;
    }

    public PipelineContinuation CurrentState { get; set; }

    public bool ResumeFrom<T>() where T : IPipelineContributor
    {
      throw new NotSupportedException("The old pipeline is dead, suspend and resume is no longer available.");
    }

    public void SuspendAfter<T>() where T : IPipelineContributor
    {
      throw new NotSupportedException("The old pipeline is dead, suspend and resume is no longer available.");
    }

    public IEnumerator<ContributorCall> GetEnumerator()
    {
      throw new NotSupportedException("The old pipeline is dead, suspend and resume is no longer available.");
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}