using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.OperationModel
{
  public class AmbiguousRequestException : Exception
  {
    public AmbiguousRequestException(IEnumerable<IOperationAsync> operations)
    {
      Message = "The request has multiple operations it could match.\r\n" +
                operations.Aggregate("", (str, operation) => str + $" - {operation.Name}({RenderInputs(operation)})\r\n");
    }

    static string RenderInputs(IOperationAsync operation) =>
      string.Join(",", operation.Inputs.Select(input => input.Member.Name));

    public override string Message { get; }
  }
}
