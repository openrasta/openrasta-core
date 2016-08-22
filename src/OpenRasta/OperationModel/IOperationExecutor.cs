using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.OperationModel
{
    public interface IOperationExecutor
    {
        Task<OperationResult> Execute(IEnumerable<IOperation> operations);
    }
}
