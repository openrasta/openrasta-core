using System.Collections.Generic;
using OpenRasta.Pipeline;

namespace OpenRasta.OperationModel
{
    public interface IOperationHydrator
    {
     IEnumerable<IOperation> Process(IEnumerable<IOperation> operations);
    }
}
