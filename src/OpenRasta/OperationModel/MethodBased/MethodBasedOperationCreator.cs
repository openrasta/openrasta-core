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

      if (filters != null)
        _filterMethod = FilterMethods(filters.ToArray()).Chain();
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
      return CreateOperationCore(method)
        .Intercept(_resolver?.ResolveAll<IOperationInterceptorAsync>());
    }

    IOperationAsync CreateOperationCore(IMethod method)
    {
      var output = method.OutputMembers.Single();
      var type = output.StaticType;

      if (type.IsTask())
        return new AsyncMethod(method, _binderLocator) {Resolver = _resolver};
      if (type.IsTaskOfT(out var returnType))
      {
        return (IOperationAsync) Activator.CreateInstance(
          typeof(AsyncMethod<>)
            .MakeGenericType(returnType),
          method, _binderLocator, _resolver);
      }

      var syncMethod = new SyncMethod(method, _binderLocator) {Resolver = _resolver};
      return syncMethod.Intercept(_syncInterceptorProvider).AsAsync();
    }

    public static IEnumerable<OperationDescriptor> CreateOperationDescriptors(
      IEnumerable<IType> handlers,
      Func<IEnumerable<IMethod>, IEnumerable<IMethod>> filters)
    {
      return from handler in handlers
        from method in filters(handler.GetMethods())
        select CreateOperationDescriptor(method, TODO, TODO, TODO);


    }

    static OperationDescriptor CreateOperationDescriptor(IMethod method,
      Func<IOperation, IEnumerable<IOperationInterceptor>> syncInterceptorProvider, IObjectBinderLocator binderLocator,
      IDependencyResolver resolver)
    {
      var type = method.OutputMembers.Single().StaticType;
      if (type.IsTask())
        return new OperationDescriptor(method.Name, AsyncMethod(method, _binderLocator) {Resolver = _resolver};
      if (type.IsTaskOfT(out var returnType))
      {
        return (IOperationAsync) Activator.CreateInstance(
          typeof(AsyncMethod<>)
            .MakeGenericType(returnType),
          method, _binderLocator, _resolver);
      }

      return new SyncOperationDescriptor(method, () =>
      {
        var syncMethod = new SyncMethod(method, binderLocator) {Resolver = resolver};
        return syncMethod.Intercept(syncInterceptorProvider).AsAsync();
      });
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

  public abstract class OperationDescriptor
  {
    readonly IMethod _method;
    readonly Func<IOperationAsync> _factory;

    public OperationDescriptor(IMethod method, Func<IOperationAsync> factory)
    {
      _method = method;
      _factory = factory; 
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
    :base(method, factory)
    {
//      this.interceptors = 
//        systemInterceptors
//          .Concat(method.Owner.Type.FindAttributes<IOperationInterceptor>())
//          .Concat(method.FindAttributes<IOperationInterceptor>())
//          .Concat(
//            method.Owner.Type.FindAttributes<IOperationInterceptorProvider>()
//            .Concat(method.FindAttributes<IOperationInterceptorProvider>())
//            .SelectMany(provider => provider.GetInterceptors())
//          method.FindAttributes<IOperationInterceptor>())      
    }
  }
}