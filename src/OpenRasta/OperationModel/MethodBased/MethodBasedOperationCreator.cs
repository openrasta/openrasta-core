using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.DI;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class MethodBasedOperationCreator : IOperationCreator
  {
    readonly IObjectBinderLocator _binderLocator;
#pragma warning disable 618
    readonly Func<IOperation,IEnumerable<IOperationInterceptor>> _syncInterceptorProvider = op=>Enumerable.Empty<IOperationInterceptor>();
#pragma warning restore 618
    readonly Func<IEnumerable<IMethod>, IEnumerable<IMethod>> _filterMethod = method => method;
    readonly IDependencyResolver _resolver;

    //// TODO: Remove when support for arrays is added to containers
    public MethodBasedOperationCreator(IDependencyResolver resolver, IObjectBinderLocator binderLocator)
      : this(binderLocator,
             resolver,
             resolver.ResolveAll<IMethodFilter>().ToArray(),
             resolver.Resolve<IOperationInterceptorProvider>())
    {
    }

    public MethodBasedOperationCreator(
      IObjectBinderLocator binderLocator = null,
      IDependencyResolver resolver = null,
      IMethodFilter[] filters = null,
      IOperationInterceptorProvider syncInterceptorProvider = null)
    {
      _resolver = resolver;
      _binderLocator = binderLocator ?? new DefaultObjectBinderLocator();
      if (syncInterceptorProvider != null)
        _syncInterceptorProvider = syncInterceptorProvider.GetInterceptors;
      if (filters != null)
        _filterMethod = FilterMethods(filters).Chain();
    }

    public IEnumerable<IOperationAsync> CreateOperations(IEnumerable<IType> handlers)
    {
      return from handler in handlers
        let sourceMethods = handler.GetMethods()
        let filteredMethods = _filterMethod(sourceMethods)
        from method in filteredMethods
        select CreateOperation(method);
    }

    public IOperationAsync CreateOperation(IMethod method)
    {
      var output = method.OutputMembers.Single();
      if (IsTask(output))
        return new AsyncMethod(method, _binderLocator) {Resolver = _resolver};
      if (IsTaskOfT(output))
        return (IOperationAsync) Activator.CreateInstance(
          typeof(AsyncMethod<>)
            .MakeGenericType(output.StaticType.GenericTypeArguments),
          method, _binderLocator);
      var syncMethod = new SyncMethod(method, _binderLocator) {Resolver = _resolver};
      return syncMethod.Intercept(_syncInterceptorProvider).AsAsync();
    }

    static bool IsTaskOfT(IMember output)
    {
      return output.StaticType.IsGenericType &&
             output.StaticType.GetGenericTypeDefinition() == typeof(Task<>);
    }

    static bool IsTask(IMember output)
    {
      return output.StaticType == typeof(Task);
    }

    static IEnumerable<Func<IEnumerable<IMethod>, IEnumerable<IMethod>>> FilterMethods(IMethodFilter[] filters)
    {
      if (filters == null)
      {
        yield return inMethods => inMethods;
        yield break;
      }
      foreach (var filter in filters)
        yield return filter.Filter;
    }
  }
}