using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class AsyncMethod : AbstractMethodOperation, IOperationAsync
  {
    public IDictionary<string,object> ExtendedProperties { get; } = new Dictionary<string, object>();
    public AsyncMethod(IMethod method, IObjectBinderLocator binderLocator = null)
      : base(method, binderLocator)
    {
    }

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      var instance = CreateInstance(OwnerType, Resolver);
      var parameters = GetParameters();

      return ((Task) Method.Invoke(instance, parameters).Single())
        .ContinueWith(task => Enumerable.Empty<OutputMember>());
    }
  }
}