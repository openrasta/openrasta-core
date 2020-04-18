using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Binding;
using OpenRasta.DI;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.OperationModel.MethodBased
{
  public class MethodBasedOperationCreator : IOperationCreator
  {
#pragma warning disable 618
    readonly Func<IOperation, IEnumerable<IOperationInterceptor>> _syncInterceptorProvider;
#pragma warning restore 618

    readonly IObjectBinderLocator _binderLocator;
    readonly Func<IEnumerable<IMethod>, IEnumerable<IMethod>> _filterMethod = method => method;
    readonly IDependencyResolver _resolver;
    readonly Func<IEnumerable<IOperationInterceptorAsync>> _asyncInterceptors;

    public MethodBasedOperationCreator(
      IObjectBinderLocator binderLocator = null,
      IDependencyResolver resolver = null,
      IEnumerable<IMethodFilter> filters = null,
      IOperationInterceptorProvider syncInterceptorProvider = null,
      Func<IEnumerable<IOperationInterceptorAsync>> asyncInterceptors = null)
    {
      _asyncInterceptors = asyncInterceptors ?? Enumerable.Empty<IOperationInterceptorAsync>;
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
      return CreateOperationDescriptors(handlers, _asyncInterceptors, _filterMethod, _syncInterceptorProvider,
          _binderLocator, _resolver)
        .Select(o => o.Create())
        .ToArray();
    }

    public IEnumerable<IOperationAsync> CreateOperations(IEnumerable<Configuration.MetaModel.OperationModel> uriModel)
    {
      return uriModel.Select(o => o.Factory()).ToArray();
    }


    public static IEnumerable<OperationDescriptor> CreateOperationDescriptors(IEnumerable<IType> handlers,
      Func<IEnumerable<IOperationInterceptorAsync>> asyncInterceptors,
      Func<IEnumerable<IMethod>, IEnumerable<IMethod>> filters = null,
      Func<IOperation, IEnumerable<IOperationInterceptor>> syncInterceptors = null,
      IObjectBinderLocator binderLocator = null,
      IDependencyResolver resolver = null)
    {
      filters ??= new TypeExclusionMethodFilter<object>().Filter;

      return from handler in handlers
        from method in filters(handler.GetMethods())
        select CreateOperationDescriptor(method, asyncInterceptors, syncInterceptors, binderLocator, resolver);
    }

    public static OperationDescriptor CreateOperationDescriptor(IMethod method,
      Func<IEnumerable<IOperationInterceptorAsync>> asyncInterceptors = null,
      Func<IOperation, IEnumerable<IOperationInterceptor>> syncInterceptorProvider = null,
      IObjectBinderLocator binderLocator = null,
      IDependencyResolver resolver = null)
    {
      asyncInterceptors ??= Enumerable.Empty<IOperationInterceptorAsync>;
      return CreateTaskDescriptor(method, binderLocator, resolver, asyncInterceptors)
             ?? CreateTaskOfTDescriptor(method, binderLocator, resolver, asyncInterceptors)
             ?? CreateSyncDescriptor(method,
               syncInterceptorProvider ?? (op => Enumerable.Empty<IOperationInterceptor>()), binderLocator, resolver,
               asyncInterceptors);
    }

    static SyncOperationDescriptor CreateSyncDescriptor(IMethod method,
      Func<IOperation, IEnumerable<IOperationInterceptor>> syncInterceptorProvider,
      IObjectBinderLocator binderLocator,
      IDependencyResolver resolver,
      Func<IEnumerable<IOperationInterceptorAsync>> systemInterceptors)
    {
      var attribCache = new Dictionary<Type, object[]>(3)
      {
        [typeof(IOperationInterceptor)] = WarmUpAttribCache<IOperationInterceptor>(method.Owner, method),
        [typeof(IOperationInterceptorProvider)] =
          WarmUpAttribCache<IOperationInterceptorProvider>(method.Owner, method),
        [typeof(HttpOperationAttribute)] = WarmUpAttribCache<HttpOperationAttribute>(method)
      };

      IOperationAsync factory()
      {
        var syncMethod = new SyncMethod(method, binderLocator, resolver, attribCache);
        return syncMethod.Intercept(syncInterceptorProvider).AsAsync().Intercept(systemInterceptors());
      }

      return new SyncOperationDescriptor(method, factory);
    }

    static T[] WarmUpAttribCache<T>(params IAttributeProvider[] elements) where T : class
    {
      return elements
        .Aggregate(Enumerable.Empty<T>(), (enumerable, provider) => enumerable.Concat(provider.FindAttributes<T>()))
        .ToArray();
    }

    static OperationDescriptor CreateTaskDescriptor(IMethod method,
      IObjectBinderLocator binderLocator,
      IDependencyResolver resolver,
      Func<IEnumerable<IOperationInterceptorAsync>> systemInterceptors)
    {
      if (!method.OutputMembers.Single().StaticType.IsTask())
        return null;

      var attribCache = LoadAsyncAttribCache(method);

      IOperationAsync factory() =>
        new AsyncMethod(method, binderLocator, resolver, attribCache).Intercept(systemInterceptors());

      return new AsyncOperationDescriptor(method, factory);
    }

    static OperationDescriptor CreateTaskOfTDescriptor(IMethod method,
      IObjectBinderLocator binderLocator,
      IDependencyResolver resolver,
      Func<IEnumerable<IOperationInterceptorAsync>> asyncInterceptors)
    {
      if (!method.OutputMembers.Single().StaticType.IsTaskOfT(out var returnType))
        return null;

      var attribCache = LoadAsyncAttribCache(method);

      var asyncMethodType = typeof(AsyncMethod<>).MakeGenericType(returnType);

      IOperationAsync factory()
      {
        return ((IOperationAsync) Activator.CreateInstance(asyncMethodType, method, binderLocator, resolver,
            attribCache))
          .Intercept(asyncInterceptors());
      }

      return new AsyncOperationDescriptor(method, factory);
    }

    static Dictionary<Type, object[]> LoadAsyncAttribCache(IMethod method)
    {
      return new Dictionary<Type, object[]>(3)
      {
        [typeof(IOperationInterceptorAsync)] = WarmUpAttribCache<IOperationInterceptorAsync>(method.Owner, method),
        [typeof(HttpOperationAttribute)] = WarmUpAttribCache<HttpOperationAttribute>(method)
      };
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
      return CreateOperationDescriptor(method, _asyncInterceptors, _syncInterceptorProvider, _binderLocator, _resolver)
        .Create();
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
      HttpOperationAttribute = _method.FindAttribute<HttpOperationAttribute>();
    }

    public string Name => _method.Name;
    public HttpOperationAttribute HttpOperationAttribute { get; }

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
    public SyncOperationDescriptor(IMethod method, Func<IOperationAsync> factory)
      : base(method, factory)
    {
    }
  }
}