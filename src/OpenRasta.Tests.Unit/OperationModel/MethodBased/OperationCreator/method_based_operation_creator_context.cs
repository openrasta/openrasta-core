using System.Collections.Generic;
using System.Linq;
using OpenRasta.Binding;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.TypeSystem;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased
{
  public abstract class method_based_operation_creator_context : operation_creator_context<MethodBasedOperationCreator>
  {
    protected IList<IType> Handlers { get; set; }
    protected IEnumerable<IOperationAsync> Operations { get; set; }

    protected void then_operation_count_should_be(int count)
    {
      Operations.Count().ShouldBe(count);
    }

    protected void given_operation_creator(IMethodFilter[] filters)
    {
      OperationCreator = new MethodBasedOperationCreator(filters: filters, resolver: Resolver);
    }

    protected void given_handler<T>()
    {
      Handlers = Handlers ?? new List<IType>();
      Handlers.Add(TypeSystem.FromClr<T>());
    }

    protected void when_creating_operations()
    {
      Operations = OperationCreator.CreateOperations(Handlers);
    }
  }
}
