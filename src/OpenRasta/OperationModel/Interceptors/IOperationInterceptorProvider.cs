#pragma warning disable 618
using System.Collections.Generic;

namespace OpenRasta.OperationModel.Interceptors
{
  public interface IOperationInterceptorProvider
  {
    IEnumerable<IOperationInterceptor> GetInterceptors(IOperation operation);
  }
}

#pragma warning restore 618