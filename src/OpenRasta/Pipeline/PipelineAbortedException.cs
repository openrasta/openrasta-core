using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Pipeline
{
  public class PipelineAbortedException : Exception
  {
    public PipelineAbortedException(IEnumerable<Error> errors)
    {
      this.Errors = errors.ToList();
    }

    public IEnumerable<Error> Errors { get; }

    public PipelineAbortedException()
    {
      this.Errors = new List<Error>();
    }
  }
}