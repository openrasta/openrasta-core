using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenRasta.DI.Internal
{
  public class DependencyRegistration
  {
    readonly DependencyLifetimeManager _lifetimeManager;

    public DependencyRegistration(Type serviceType, Type concreteType, DependencyLifetimeManager lifetime,
      object instance = null)
    {
      Key = Guid.NewGuid().ToString();
      _lifetimeManager = lifetime;
      ServiceType = serviceType;
      ConcreteType = concreteType;
      Constructors =
        new List<KeyValuePair<ConstructorInfo, ParameterInfo[]>>(
          concreteType.GetConstructors()
            .Select(ctor => new KeyValuePair<ConstructorInfo, ParameterInfo[]>(ctor, ctor.GetParameters())));
      Constructors.Sort((kv1, kv2) => kv1.Value.Length.CompareTo(kv2.Value.Length) * -1);
      Instance = instance;
      IsInstanceRegistration = instance != null;
      
      _lifetimeManager.Add(this);
    }

    public Type ConcreteType { get; }
    public List<KeyValuePair<ConstructorInfo, ParameterInfo[]>> Constructors { get; }
    public object Instance { get; set; }
    public bool IsInstanceRegistration { get; }
    public string Key { get; }

    public Type ServiceType { get; }

    public bool IsRegistrationAvailable
      => _lifetimeManager.Contains(this);

    public object ResolveInContext(ResolveContext ctx)
      => _lifetimeManager.Resolve(ctx, this);
  }
}