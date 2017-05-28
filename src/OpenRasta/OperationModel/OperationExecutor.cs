using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.OperationModel
{
  public class OperationExecutor : IOperationExecutor
  {
    public async Task<OperationResult> Execute(IEnumerable<IOperationAsync> operations)
    {
      var operation = operations.First();
      var result = (await operation.InvokeAsync()).Select(_ => _.Value).FirstOrDefault();

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