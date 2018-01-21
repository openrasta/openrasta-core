using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenRasta.Diagnostics;

namespace OpenRasta.DI.Internal
{
  public class ObjectBuilder
  {
    public ObjectBuilder(ResolveContext context)
    {
      ResolveContext = context;
    }

    ILogger Log { get; } = TraceSourceLogger.Instance;

    ResolveContext ResolveContext { get; }

    public object CreateObject(DependencyRegistration registration)
    {
      StringBuilder unresolvedDependenciesMessage = null;
      foreach (var constructor in registration.Constructors)
      {
        var unresolvedDependencies = new List<ParameterInfo>();
        var dependents = constructor.Value
          .Select(pi =>
          {
            var success = ResolveContext.TryResolve(pi.ParameterType, out var instance);
            if (!success) unresolvedDependencies.Add(pi);
            return instance;
          }).ToArray();


        if (unresolvedDependencies.Any() == false) return AssignProperties(constructor.Key.Invoke(dependents));

        LogUnresolvedConstructor(unresolvedDependencies, ref unresolvedDependenciesMessage);
      }

      throw new DependencyResolutionException(
        $"Could not resolve type {registration.ConcreteType.Name} because its dependencies couldn't be fullfilled\r\n{unresolvedDependenciesMessage}");
    }

    object AssignProperties(object instanceObject)
    {
      foreach (var property in from pi in instanceObject.GetType().GetProperties()
        where pi.CanWrite && pi.GetIndexParameters().Length == 0
        let resolve = ResolveProperty(pi)
        where resolve.success
        select resolve)
        property.pi.SetValue(instanceObject, property.instance, null);

      return instanceObject;
    }

    (PropertyInfo pi, bool success, object instance) ResolveProperty(PropertyInfo pi)
    {
      var success = ResolveContext.TryResolve(pi.PropertyType, out var instance);
      return (pi, success, instance);
    }

    void LogUnresolvedConstructor(IEnumerable<ParameterInfo> unresolvedDependencies,
      ref StringBuilder unresolvedDependenciesMessage)
    {
      unresolvedDependenciesMessage = unresolvedDependenciesMessage ?? new StringBuilder();
      var message = unresolvedDependencies.Aggregate(string.Empty,
        (str, pi) => str + pi.ParameterType);
      Log.WriteDebug("Ignoring constructor, following dependencies didn't have a registration:" + message);
      unresolvedDependenciesMessage.Append("Constructor: ").AppendLine(message);
    }
  }
}