using OpenRasta.OperationModel;
using OpenRasta.Tests.Unit.Infrastructure;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased
{
  public abstract class operation_creator_context<T> : openrasta_context
    where T : IOperationCreator
  {
    protected T OperationCreator { get; set; }
  }
}