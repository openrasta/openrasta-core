using System.Collections.Generic;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel
{
    public interface IOperationCreator
    {
        IEnumerable<IOperationAsync> CreateOperations(IEnumerable<IType> handlers);
    }
}