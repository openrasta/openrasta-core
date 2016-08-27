using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Pipeline;

namespace OpenRasta.OperationModel
{
    public interface IOperationHydrator //: IOperationProcessor<KnownStages.IRequestDecoding>
    {
     IEnumerable<IOperation> Process(IEnumerable<IOperation> operations);
    }

  public interface IRequestEntityReader
  {
    Task<IOperation> Read(IEnumerable<IOperation> operation);
  }
}
