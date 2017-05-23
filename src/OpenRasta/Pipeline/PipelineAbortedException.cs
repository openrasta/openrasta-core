using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Pipeline
{
  public class PipelineAbortedException : AggregateException //: AggregateException
  {
    public PipelineAbortedException(IEnumerable<Error> errors)
      // : base(errors.Select(error => error.Exception).ToList())
    {
      Errors = errors.ToList();
    }

    public IEnumerable<Error> Errors { get; }

    public PipelineAbortedException()
    {
      Errors = new List<Error>();
    }
  }
}