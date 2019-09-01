using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenRasta.Pipeline
{
  public class PipelineAbortedException : Exception
  {
    // ReSharper disable once MemberCanBePrivate.Global
    public IEnumerable<Error> Errors { get; } = new List<Error>();

    public PipelineAbortedException()
      : base("The pipeline aborted for an unknown reason")
    {
    }

    public PipelineAbortedException(IEnumerable<Error> errors)
      : this(errors.ToList())
    {
    }

    PipelineAbortedException(List<Error> errors)
      : base(GenerateMessage(errors), GenerateInnerException(errors))
    {
      Errors = errors;
    }

    static Exception GenerateInnerException(IEnumerable<Error> errors)
    {
      var exceptions = errors.Where(e => e.Exception != null).Select(e => e.Exception).ToList();
      if (exceptions.Count == 1)
        return exceptions[0];
      return new AggregateException(exceptions).Flatten();
    }

    static string GenerateMessage(IEnumerable<Error> errors)
    {
      return string.Join(Environment.NewLine, errors.Select(e => e.ToStringWithoutException()));
    }
  }
}