using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel
{
  public static class OperationExtensions
  {
    public static bool AllReady(this IEnumerable<InputMember> members)
    {
      return members.All(x => x.IsReadyForAssignment);
    }

    public static IEnumerable<IMember> WhosNotReady(this IEnumerable<InputMember> members)
    {
      return members.Where(x => x.IsReadyForAssignment == false).Select(x => x.Member);
    }

    /// <summary>
    /// Returns the number of members ready for invocation (aka either having a default value or having had a value assigned to them).
    /// </summary>
    /// <param name="members"></param>
    /// <returns></returns>
    public static int CountReady(this IEnumerable<InputMember> members)
    {
      return members.Count(x => x.IsReadyForAssignment);
    }

    /// <summary>
    /// Returns a list of members not required for an operation to execute.
    /// </summary>
    /// <param name="members"></param>
    /// <returns></returns>
    public static IEnumerable<InputMember> Optional(this IEnumerable<InputMember> members)
    {
      return members.Where(x => x.IsOptional);
    }

    /// <summary>
    /// Returns the list of members required for an operation to execute.
    /// </summary>
    /// <param name="members"></param>
    /// <returns></returns>
    public static IEnumerable<InputMember> Required(this IEnumerable<InputMember> members)
    {
      return members.Where(x => !x.IsOptional);
    }

#pragma warning disable 618

    public static IOperationAsync AsAsync(this IOperation operation)
    {
      return new SyncToAsyncOperation(operation);
    }

    public static IOperation SyncOperation(this IOperationAsync operation)
    {
      object sync;
      return operation.ExtendedProperties.TryGetValue("openrasta.SyncOperation", out sync)
        ? (IOperation) sync
        : null;
    }

    public static IOperation Intercept(this IOperation operation,
      Func<IOperation, IEnumerable<IOperationInterceptor>> interceptors)
    {
      return new SyncOperationWithInterceptors(operation, interceptors(operation));
    }

#pragma warning restore 618

    public static IOperationAsync Intercept(this IOperationAsync operation, IEnumerable<IOperationInterceptorAsync> systemInterceptors = null)
    {
      return new AsyncOperationWithInterceptors(operation, systemInterceptors ?? Enumerable.Empty<IOperationInterceptorAsync>());
    }

    public static bool IsTaskOfT(this Type type, out Type returnType)
    {
      if (type.IsGenericType &&
          type.GetGenericTypeDefinition() == typeof(Task<>))
      {
        returnType = type.GetGenericArguments()[0];
        return true;
      }

      returnType = null;
      return false;
    }

    public static bool IsTask(this Type type)
    {
      return type == typeof(Task);
    }
  }
}