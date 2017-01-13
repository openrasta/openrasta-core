using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.OperationModel.Hydrators;

namespace OpenRasta.OperationModel
{
  public interface IRequestEntityReader
  {
    Task<Tuple<RequestReadResult, IOperationAsync>> Read(IEnumerable<IOperationAsync> operations);
  }
}