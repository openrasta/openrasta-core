#pragma warning disable 618
using System;
using System.Collections.Generic;

namespace OpenRasta.OperationModel.Interceptors
{
  [Obsolete]
  public abstract class InterceptorProviderAttribute : Attribute, IOperationInterceptorProvider
  {
    public abstract IEnumerable<IOperationInterceptor> GetInterceptors(IOperation operation);
  }
}

#pragma warning restore 618