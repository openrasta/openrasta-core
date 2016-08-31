using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel
{
  public interface IOperation : IAttributeProvider
  {
    IEnumerable<InputMember> Inputs { get; }
    IDictionary ExtendedProperties { get; }
    string Name { get; }
    [Obsolete("Please use InvokeAsync")]
    IEnumerable<OutputMember> Invoke();
  }

  public interface IOperationAsync : IOperation
  {
    Task<IEnumerable<OutputMember>> InvokeAsync();
  }
}
