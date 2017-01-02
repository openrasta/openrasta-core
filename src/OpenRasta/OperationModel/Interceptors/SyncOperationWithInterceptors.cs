using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.OperationModel.Interceptors
{
  public class SyncOperationWithInterceptors : IOperation
  {
    readonly IEnumerable<IOperationInterceptor> _interceptors;
    readonly IOperation _inner;
    Func<IEnumerable<OutputMember>> _invocation;

    public SyncOperationWithInterceptors(IOperation inner, IEnumerable<IOperationInterceptor> systemInterceptors)
    {

      _inner = inner;
      _interceptors = systemInterceptors.ToList();
      _invocation = _interceptors.Aggregate((Func<IEnumerable<OutputMember>>) _inner.Invoke,
        (next, interceptor) => interceptor.RewriteOperation(next));
    }

    public IDictionary ExtendedProperties => _inner.ExtendedProperties;
    public IEnumerable<InputMember> Inputs => _inner.Inputs;
    public string Name => _inner.Name;
    public T FindAttribute<T>() where T : class => _inner.FindAttribute<T>();
    public IEnumerable<T> FindAttributes<T>() where T : class => _inner.FindAttributes<T>();
    public override string ToString() => _inner.ToString();

    public IEnumerable<OutputMember> Invoke()
    {
      ExecutePreConditions();
      var results = _invocation().ToList();
      ExecutePostConditions(results);
      return results;
    }

    void ExecutePostConditions(IEnumerable<OutputMember> results)
    {
      foreach (var postCondition in _interceptors)
      {
        TryExecute(() => postCondition.AfterExecute(_inner, results),
          $"The interceptor {postCondition.GetType().Name} stopped execution.");
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
        var isSuccessful = interception();
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