using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.Collections;
using OpenRasta.DI;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class MethodBasedOperation : IOperation, IOperationAsync
  {
    readonly IMethod _method;
    readonly IType _ownerType;

    readonly Dictionary<IParameter, IObjectBinder> _parameterBinders;
    IOperationInvoker _invoker;

    public MethodBasedOperation(IObjectBinderLocator binderLocator, IType ownerType, IMethod method)
    {
      _method = method;
      _ownerType = ownerType;
      _parameterBinders = method.InputMembers.ToDictionary(x => x, binderLocator.GetBinder);
      Inputs = _parameterBinders.Select(x => new InputMember(x.Key, x.Value, x.Key.IsOptional));
      ExtendedProperties = new NullBehaviorDictionary<object, object>();
      _invoker = CreateInvoker(_method);
    }

    static IOperationInvoker CreateInvoker(IMethod method)
    {
      var output = method.OutputMembers.Single();
      if (output.StaticType == typeof(Task))
        return new TaskOperationInvoker(method);
      if (output.StaticType.IsGenericType &&
          output.StaticType.GetGenericTypeDefinition() == typeof(Task<>))
        return (IOperationInvoker) Activator.CreateInstance(
          typeof(TaskOperationInvoker<>).MakeGenericType(output.StaticType.GetGenericArguments()[0]),
          method);
      return new SyncOperationInvoker(method);
    }

    public IDictionary ExtendedProperties { get; private set; }
    public IEnumerable<InputMember> Inputs { get; private set; }

    public string Name => _method.Name;

    public IDependencyResolver Resolver { private get; set; }

    public override string ToString()
    {
      return _method.ToString();
    }

    public T FindAttribute<T>() where T : class
    {
      return _method.FindAttribute<T>() ?? _ownerType.FindAttribute<T>();
    }

    public IEnumerable<T> FindAttributes<T>() where T : class
    {
      return _ownerType.FindAttributes<T>().Concat(_method.FindAttributes<T>());
    }

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      if (!Inputs.AllReady())
      {
        var notReady = Inputs.WhosNotReady();
        throw new InvalidOperationException(
          $"'{_method.Owner.Name}.{_method.Name} could not execute. " +
          $"These members have not been provided: {notReady.Select(x => x.Name).JoinString(", ")}");
      }

      var handler = CreateInstance(_ownerType, Resolver);

      var bindingResults = from kv in _parameterBinders
        let param = kv.Key
        let binder = kv.Value
        select binder.IsEmpty
          ? BindingResult.Success(param.DefaultValue)
          : binder.BuildObject();

      var parameters = GetParameters(bindingResults);

      return _invoker.Invoke(handler, parameters.ToArray());
    }

    public IEnumerable<OutputMember> Invoke()
    {
      return InvokeAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Returns an instance of the type, optionally through the container if it is supported.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="resolver"></param>
    /// <returns></returns>
    static object CreateInstance(IType type, IDependencyResolver resolver)
    {
      var typeForResolver = type as IResolverAwareType;
      return resolver == null || typeForResolver == null
        ? type.CreateInstance()
        : typeForResolver.CreateInstance(resolver);
    }

    IEnumerable<object> GetParameters(IEnumerable<BindingResult> results)
    {
      foreach (var result in results)
        if (!result.Successful)
          throw new InvalidOperationException("A parameter wasn't successfully created.");
        else
          yield return result.Instance;
    }
  }


  class TaskOperationInvoker<T> : IOperationInvoker
  {
    readonly IMethod _method;
    readonly Func<object, Task<T>> _invoke;

    public TaskOperationInvoker(IMethod method)
    {
      _method = method;
      _invoke = _ => (Task<T>) method.Invoke(_).Single();
    }

    public Task<IEnumerable<OutputMember>> Invoke(object instance, object[] parameters)
    {
      return _invoke(instance).ContinueWith(_ =>
        (IEnumerable<OutputMember>) new[]
        {
          new OutputMember
          {
            Member = _method.OutputMembers.Single(),
            Value = _.Result
          }
        });
    }
  }

  class TaskOperationInvoker : IOperationInvoker
  {
    readonly Func<object, Task> _invoke;

    public TaskOperationInvoker(IMethod method)
    {
      _invoke = _ => (Task) method.Invoke(_).Single();
    }

    public Task<IEnumerable<OutputMember>> Invoke(object instance, object[] parameters)
    {
      return _invoke(instance).ContinueWith(_ => Enumerable.Empty<OutputMember>());
    }
  }

  class SyncOperationInvoker : IOperationInvoker
  {
    readonly IMethod _method;

    public SyncOperationInvoker(IMethod method)
    {
      _method = method;
    }

    public Task<IEnumerable<OutputMember>> Invoke(object instance, object[] parameters)
    {
      return Task.FromResult<IEnumerable<OutputMember>>(
        new[]
        {
          new OutputMember()
          {
            Member = _method.OutputMembers.Single(),
            Value = _method.Invoke(instance, parameters)
          }
        });
    }
  }

  internal interface IOperationInvoker
  {
    Task<IEnumerable<OutputMember>> Invoke(object instance, object[]
      parameters);
  }
}
