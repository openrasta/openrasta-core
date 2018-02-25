using System;
using System.Collections.Generic;
using System.Linq;
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

    readonly Func<IOperation, IEnumerable<IOperationInterceptor>> _syncInterceptorProvider =
      op => Enumerable.Empty<IOperationInterceptor>();

#pragma warning restore 618
    readonly Func<IEnumerable<IMethod>, IEnumerable<IMethod>> _filterMethod = method => method;
    readonly IDependencyResolver _resolver;

    public MethodBasedOperationCreator(
      IObjectBinderLocator binderLocator = null,
      IDependencyResolver resolver = null,
      IEnumerable<IMethodFilter> filters = null,
      IOperationInterceptorProvider syncInterceptorProvider = null)
    {
      _resolver = resolver;
      _binderLocator = binderLocator ?? new DefaultObjectBinderLocator();
      if (syncInterceptorProvider != null)
        _syncInterceptorProvider = syncInterceptorProvider.GetInterceptors;
      else
        _syncInterceptorProvider = op => Enumerable.Empty<IOperationInterceptor>();

      if (filters != null)
        _filterMethod = FilterMethods(filters.ToArray()).Chain();
    }

    public IEnumerable<IOperationAsync> CreateOperations(IEnumerable<IType> handlers)
    {
      return CreateOperationDescriptors(handlers, _filterMethod, _syncInterceptorProvider, _binderLocator, _resolver)
        .Select(o => o.Create())
        .ToArray();
    }


    public static IEnumerable<OperationDescriptor> CreateOperationDescriptors(IEnumerable<IType> handlers,
      Func<IEnumerable<IMethod>, IEnumerable<IMethod>> filters = null,
      Func<IOperation, IEnumerable<IOperationInterceptor>> syncInterceptors = null,
      IObjectBinderLocator binderLocator = null,
      IDependencyResolver resolver = null)
    {
      filters = filters ?? (m => m);
      
      return from handler in handlers
        from method in filters(handler.GetMethods())
        select CreateOperationDescriptor(method, syncInterceptors, binderLocator, resolver);
    }

    public static OperationDescriptor CreateOperationDescriptor(
      IMethod method,
      Func<IOperation, IEnumerable<IOperationInterceptor>> syncInterceptorProvider = null,
      IObjectBinderLocator binderLocator = null,
      IDependencyResolver resolver = null)
    {
      return CreateTaskDescriptor(method, binderLocator, resolver)
             ?? CreateTaskOfTDescriptor(method, binderLocator, resolver)
             ?? CreateSyncDescriptor(method, syncInterceptorProvider ?? (op => Enumerable.Empty<IOperationInterceptor>()), binderLocator, resolver);
    }

    static SyncOperationDescriptor CreateSyncDescriptor(IMethod method,
      Func<IOperation, IEnumerable<IOperationInterceptor>> syncInterceptorProvider,
      IObjectBinderLocator binderLocator,
      IDependencyResolver resolver)
    {
      return new SyncOperationDescriptor(method, () =>
      {
        var syncMethod = new SyncMethod(method, binderLocator, resolver);
        return syncMethod.Intercept(syncInterceptorProvider).AsAsync();
      });
    }

    static OperationDescriptor CreateTaskDescriptor(IMethod method, IObjectBinderLocator binderLocator,
      IDependencyResolver resolver)
    {
      if (!method.OutputMembers.Single().StaticType.IsTask())
        return null;
      
      return new AsyncOperationDescriptor(method, () => new AsyncMethod(method, binderLocator, resolver));
    }

    static OperationDescriptor CreateTaskOfTDescriptor(
      IMethod method,
      IObjectBinderLocator binderLocator,
      IDependencyResolver resolver)
    {
      if (!method.OutputMembers.Single().StaticType.IsTaskOfT(out var returnType))
        return null;
      
      return new AsyncOperationDescriptor(method
        , () => (IOperationAsync) Activator.CreateInstance(
          typeof(AsyncMethod<>).MakeGenericType(returnType),
          method, binderLocator, resolver));

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

    public IOperationAsync CreateOperation(IMethod method)
    {
      return CreateOperationDescriptor(method,_syncInterceptorProvider, _binderLocator, _resolver).Create();
    }
  }

  public abstract class OperationDescriptor
  {
    readonly IMethod _method;
    readonly Func<IOperationAsync> _factory;

    protected OperationDescriptor(IMethod method, Func<IOperationAsync> factory)
    {
      _method = method;
      _factory = factory;
    }

    public IOperationAsync Create()
    {
      return _factory();
    }
  }

  public class AsyncOperationDescriptor : OperationDescriptor
  {
    public AsyncOperationDescriptor(IMethod method, Func<IOperationAsync> factory) : base(method, factory)
    {
    }
  }

  public class SyncOperationDescriptor : OperationDescriptor
  {
    public SyncOperationDescriptor(
      IMethod method, Func<IOperationAsync> factory)
      : base(method, factory)
    {
    }
  }
}