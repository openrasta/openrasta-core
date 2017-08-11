using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenRasta.OperationModel
{
#pragma warning disable 618
  public class PretendingToBeSyncOperationForLegacyInterceptors : IOperation
#pragma warning restore 618
  {
    private readonly IOperationAsync _operationImplementation;

    public PretendingToBeSyncOperationForLegacyInterceptors(IOperationAsync operationImplementation)
    {
      _operationImplementation = operationImplementation;
    }

    public T FindAttribute<T>() where T : class => _operationImplementation.FindAttribute<T>();

    public IEnumerable<T> FindAttributes<T>() where T : class => _operationImplementation.FindAttributes<T>();

    public IEnumerable<InputMember> Inputs => _operationImplementation.Inputs;

    public IDictionary ExtendedProperties => throw new NotSupportedException("");

    public string Name => _operationImplementation.Name;

    public IEnumerable<OutputMember> Invoke()
    {
      throw new NotImplementedException();
    }
  }
}