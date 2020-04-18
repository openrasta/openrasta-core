using System.Collections.Generic;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel
{
  public interface IOperationCreator
  {
    IEnumerable<IOperationAsync> CreateOperations(IEnumerable<IType> handlers);
    IEnumerable<IOperationAsync> CreateOperations(IEnumerable<Configuration.MetaModel.OperationModel> uriModel);
  }
}