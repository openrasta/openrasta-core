using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenRasta.OperationModel
{
  public interface IOperationAsync : IOperation
  {
    Task<IEnumerable<OutputMember>> InvokeAsync();
  }
}