using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenRasta.OperationModel.Interceptors
{
  public class OperationWithInterceptors : IOperation, IOperationAsync
  {
    readonly IEnumerable<IOperationInterceptor> _interceptors;
    readonly IOperationAsync _inner;
    Task<IEnumerable<OutputMember>> _task;

    public OperationWithInterceptors(IOperationAsync inner, IEnumerable<IOperationInterceptor> systemInterceptors)
    {
      _inner = inner;
      _interceptors = systemInterceptors;
    }

    public IDictionary ExtendedProperties => _inner.ExtendedProperties;

    public IEnumerable<InputMember> Inputs => _inner.Inputs;

    public string Name => _inner.Name;
    public IEnumerable<OutputMember> Invoke()
    {
      return InvokeAsync().GetAwaiter().GetResult();
    }

    public T FindAttribute<T>() where T : class => _inner.FindAttribute<T>();

    public IEnumerable<T> FindAttributes<T>() where T : class => _inner.FindAttributes<T>();

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      ExecutePreConditions();

      _task = Task.Run(()=>_inner.InvokeAsync());
      var rewrite = _interceptors.Aggregate<IOperationInterceptor, Func<IEnumerable<OutputMember>>>(
        () => _task.GetAwaiter().GetResult(),
        (current, executingCondition) => executingCondition.RewriteOperation(current) ?? current);

      return _task.ContinueWith(_ =>
      {
        var results = rewrite().ToList().AsEnumerable();
        ExecutePostConditions(results);
        return Task.FromResult(results);
      }).Unwrap();
    }

    void ExecutePostConditions(IEnumerable<OutputMember> results)
    {
      foreach (var postCondition in _interceptors)
      {
        TryExecute(() => postCondition.AfterExecute(_inner, results),
          "The interceptor {0} stopped execution.".With(postCondition.GetType().Name));
      }
    }

    void ExecutePreConditions()
    {
      foreach (var precondition in _interceptors)
      {
        TryExecute(() => precondition.BeforeExecute(_inner),
          "The interceptor {0} stopped execution.".With(precondition.GetType().Name));
      }
    }

    void TryExecute(Func<bool> interception, string exceptionMessage)
    {
      Exception exception = null;
      try
      {
        bool isSuccessful = interception();
        if (!isSuccessful)
        {
          exception = new InterceptorException(exceptionMessage);
        }
      }
      catch (Exception e)
      {
        exception = new InterceptorException(exceptionMessage, e);
      }
      if (exception != null)
      {
        throw exception;
      }
    }
  }
}
