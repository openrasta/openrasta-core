using System;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.OperationModel.MethodBased
{
  public abstract class OperationDescriptor
  {
    readonly IMethod _method;
    readonly Func<IOperationAsync> _factory;

    protected OperationDescriptor(IMethod method, Func<IOperationAsync> factory)
    {
      _method = method;
      _factory = factory;
      HttpOperationAttribute = _method.FindAttribute<HttpOperationAttribute>();
    }

    public string Name => _method.Name;
    public HttpOperationAttribute HttpOperationAttribute { get; }

    public IOperationAsync Create()
    {
      return _factory();
    }
  }
}