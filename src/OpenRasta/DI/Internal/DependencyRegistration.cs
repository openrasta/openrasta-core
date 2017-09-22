using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenRasta.DI.Internal
{
  public class DependencyRegistration : IEquatable<DependencyRegistration>
  {
    public DependencyLifetimeManager LifetimeManager { get; }

    public bool Equals(DependencyRegistration other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(Key, other.Key);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((DependencyRegistration) obj);
    }

    public override int GetHashCode()
    {
      return Key.GetHashCode();
    }

    public static bool operator ==(DependencyRegistration left, DependencyRegistration right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(DependencyRegistration left, DependencyRegistration right)
    {
      return !Equals(left, right);
    }

    public DependencyRegistration(Type serviceType, Type concreteType, DependencyLifetime lifetime, DependencyLifetimeManager lifetimeManager,
      object instance = null)
    {
      Key = Guid.NewGuid().ToString();
      LifetimeManager = lifetimeManager;
      ServiceType = serviceType;
      ConcreteType = concreteType;
      Lifetime = lifetime;
      Constructors =
        new List<KeyValuePair<ConstructorInfo, ParameterInfo[]>>(
          concreteType.GetConstructors()
            .Select(ctor => new KeyValuePair<ConstructorInfo, ParameterInfo[]>(ctor, ctor.GetParameters())));
      Constructors.Sort((kv1, kv2) => kv1.Value.Length.CompareTo(kv2.Value.Length) * -1);
      _instance = instance;
      IsInstanceRegistration = instance != null;
      
      LifetimeManager.Add(this);
    }

    public Type ConcreteType { get; }
    public DependencyLifetime Lifetime { get; }
    public List<KeyValuePair<ConstructorInfo, ParameterInfo[]>> Constructors { get; }
    private readonly object _instance;
    public bool IsInstanceRegistration { get; }
    public string Key { get; }

    public Type ServiceType { get; }

    public bool IsRegistrationAvailable
      => LifetimeManager.Contains(this);

    public object ResolveInContext(ResolveContext ctx)
      => LifetimeManager.Resolve(ctx, this);

    public Func<ResolveContext, object> CreateInstance => CreateInstanceImplementation;
    public object Instance => _instance;

    private object CreateInstanceImplementation(ResolveContext context) =>
      IsInstanceRegistration ? _instance : new ObjectBuilder(context).CreateObject(this);

  }
}