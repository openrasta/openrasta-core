using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel
{
  public interface IOperationAsync : IAttributeProvider
  {
    IEnumerable<InputMember> Inputs { get; }
    IDictionary<string,object> ExtendedProperties { get; }
    string Name { get; }
    Task<IEnumerable<OutputMember>> InvokeAsync();
  }
}