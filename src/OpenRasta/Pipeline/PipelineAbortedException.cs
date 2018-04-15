using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Pipeline
{
  public class PipelineAbortedException : Exception
  {
    public IEnumerable<Error> Errors { get; }

    public PipelineAbortedException(IEnumerable<Error> errors)
    {
      Errors = errors.ToList();
    }

    public PipelineAbortedException()
    {
      Errors = new List<Error>();
    }

    public override string ToString()
    {
      return $"{base.ToString()}{Environment.NewLine}" +
             string.Join(Environment.NewLine, Errors.Select(e => e.ToString()));
    }
  }
}