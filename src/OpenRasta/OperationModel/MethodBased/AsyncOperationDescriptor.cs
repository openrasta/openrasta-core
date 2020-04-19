using System;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class AsyncOperationDescriptor : OperationDescriptor
  {
    public AsyncOperationDescriptor(IMethod method, Func<IOperationAsync> factory) : base(method, factory)
    {
    }
  }
}