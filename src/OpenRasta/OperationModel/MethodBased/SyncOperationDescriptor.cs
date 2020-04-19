using System;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class SyncOperationDescriptor : OperationDescriptor
  {
    public SyncOperationDescriptor(IMethod method, Func<IOperationAsync> factory)
      : base(method, factory)
    {
    }
  }
}