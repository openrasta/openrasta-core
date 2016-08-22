using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.OperationModel
{
  public class OperationExecutor : IOperationExecutor
  {
    static readonly ConcurrentDictionary<Type, Func<object, Task<OperationResult>>> _accessor
      = new ConcurrentDictionary<Type, Func<object, Task<OperationResult>>>();

    public async Task<OperationResult> Execute(IEnumerable<IOperation> operations)
    {
      var operation = operations.Cast<IOperationAsync>().First();
      var result = (await operation.InvokeAsync()).Select(_=>_.Value).FirstOrDefault();

      return ToOperationResult(result);
    }

    static OperationResult ToOperationResult(object returnValue)
    {
      if (returnValue == null)
        return new OperationResult.NoContent();
      var operationResult = returnValue as OperationResult;
      return operationResult ?? new OperationResult.OK(returnValue);
    }
  }
}
