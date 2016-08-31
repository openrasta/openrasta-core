using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.OperationModel.Hydrators;
using OpenRasta.Pipeline;

namespace OpenRasta.OperationModel
{
    public interface IOperationHydrator //: IOperationProcessor<KnownStages.IRequestDecoding>
    {
     IEnumerable<IOperation> Process(IEnumerable<IOperation> operations);
    }

  public interface IRequestEntityReader
  {
    Task<Tuple<RequestReadResult, IOperation>> Read(IEnumerable<IOperation> operation);
  }
}
