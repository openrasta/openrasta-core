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
            var result = ResolveContext.TryResolve(pi.ParameterType);
            if (result == null) unresolvedDependencies.Add(pi);
            return result;
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
        let instance = ResolveContext.TryResolve(pi.PropertyType)
        where instance != null
        select new {pi, instance})
        property.pi.SetValue(instanceObject, property.instance, null);

      return instanceObject;
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