using System;
using System.Collections.Generic;
using OpenRasta.Pipeline;

namespace OpenRasta.OperationModel
{
    [Obsolete("This interface was replaced with IRequestEntityReader")]
    public interface IOperationHydrator
    {
     IEnumerable<IOperation> Process(IEnumerable<IOperation> operations);
    }
}
