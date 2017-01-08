using System.Collections.Generic;

namespace OpenRasta.OperationModel
{
    public interface IOperationProcessor
    {
        IEnumerable<IOperationAsync> Process(IEnumerable<IOperationAsync> operations);
    }
}