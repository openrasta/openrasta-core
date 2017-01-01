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
  public abstract class AbstractMethodOperation
  {
    public IType OwnerType { get; }
    public IMethod Method { get; }

    protected AbstractMethodOperation(IType ownerType, IMethod method, IObjectBinderLocator binderLocator)
    {
      OwnerType = ownerType;
      Method = method;
      Binders = method.InputMembers.ToDictionary(x => x, binderLocator.GetBinder);
      Inputs = Binders.Select(x => new InputMember(x.Key, x.Value, x.Key.IsOptional));
      ExtendedProperties = new NullBehaviorDictionary<object, object>();

    }

    public IDictionary ExtendedProperties { get; set; }

    public IEnumerable<InputMember> Inputs { get; set; }

    public IDictionary<IParameter, IObjectBinder> Binders { get; set; }
    public string Name => Method.Name;

    public IEnumerable<T> FindAttributes<T>() where T : class
    {
      return OwnerType.FindAttributes<T>().Concat(Method.FindAttributes<T>());
    }

    public T FindAttribute<T>() where T : class
    {
      return Method.FindAttribute<T>() ?? OwnerType.FindAttribute<T>();
    }

    public override string ToString()
    {
      return Method.ToString();
    }
  }

  public class MethodBasedOperation : AbstractMethodOperation, IOperation, IOperationAsync
  {
    IOperationInvoker _invoker;

    public MethodBasedOperation(IObjectBinderLocator binderLocator, IType ownerType, IMethod method)
      : base(ownerType, method, binderLocator)
    {
      _invoker = CreateInvoker(base.Method);
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

    public IDependencyResolver Resolver { private get; set; }


    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      if (!Inputs.AllReady())
      {
        var notReady = Inputs.WhosNotReady();
        throw new InvalidOperationException(
          $"'{Method.Owner.Name}.{Method.Name} could not execute. " +
          $"These members have not been provided: {notReady.Select(x => x.Name).JoinString(", ")}");
      }

      var handler = CreateInstance(OwnerType, Resolver);

      var bindingResults = from kv in Binders
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

    static IEnumerable<object> GetParameters(IEnumerable<BindingResult> results)
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
