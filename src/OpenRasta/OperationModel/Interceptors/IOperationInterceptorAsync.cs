using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenRasta.OperationModel.Interceptors
{
  public interface IOperationInterceptorAsync
  {
    Func<IOperationAsync, Task<IEnumerable<OutputMember>>> Invoke(
      Func<IOperationAsync, Task<IEnumerable<OutputMember>>> next);
  }
}