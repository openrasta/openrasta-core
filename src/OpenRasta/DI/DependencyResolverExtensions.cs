using System;

namespace OpenRasta.DI
{
  public static class DependencyResolverExtensions
  {
    /// <summary>
    /// Adds a concrete dependency to the resolver.
    /// </summary>
    /// <typeparam name="TConcrete">The concrete type to register.</typeparam>
    /// <param name="resolver"></param>
    public static void AddDependency<TConcrete>(this IDependencyResolver resolver)
      where TConcrete : class
    {
      AddDependency<TConcrete>(resolver, DependencyLifetime.Transient);
    }

    /// <summary>
    /// Adds a concrete dependency with the specified lifetime.
    /// </summary>
    /// <typeparam name="TConcrete">The concrete type to register.</typeparam>
    /// <param name="resolver"></param>
    /// <param name="lifetime">The lifetime of the type.</param>
    public static void AddDependency<TConcrete>(this IDependencyResolver resolver, DependencyLifetime lifetime)
      where TConcrete : class
    {
      resolver.AddDependency(typeof(TConcrete), lifetime);
    }

    /// <summary>
    /// Adds a dependency of type <typeparamref name="TService"/>, implemented by the type <typeparamref name="TConcrete"/>.
    /// </summary>
    /// <typeparam name="TService">The type to register.</typeparam>
    /// <typeparam name="TConcrete">The type of the concrete implementation.</typeparam>
    /// <param name="resolver">The resolver.</param>
    public static void AddDependency<TService, TConcrete>(this IDependencyResolver resolver)
      where TService : class
      where TConcrete : class, TService
    {
      AddDependency<TService, TConcrete>(resolver, DependencyLifetime.Singleton);
    }

    /// <summary>
    /// Adds a dependency of type <typeparamref name="TService"/>, implemented by the type <typeparamref name="TConcrete"/>, with the specified dependency lifetime.
    /// </summary>
    /// <typeparam name="TService">The type to register.</typeparam>
    /// <typeparam name="TConcrete">The type of the concrete implementation.</typeparam>
    /// <param name="resolver">The resolver.</param>
    /// <param name="lifetime">The lifetime of the type.</param>
    public static void AddDependency<TService, TConcrete>(this IDependencyResolver resolver,
      DependencyLifetime lifetime)
      where TService : class
      where TConcrete : class, TService
    {
      resolver.AddDependency(typeof(TService), typeof(TConcrete), lifetime);
    }

    public static void AddDependencyInstance(this IDependencyResolver resolver, Type serviceType, object instance)
    {
      resolver.AddDependencyInstance(serviceType, instance, DependencyLifetime.Singleton);
    }

    public static void AddDependencyInstance<TService>(this IDependencyResolver resolver, object instance)
    {
      resolver.AddDependencyInstance(typeof(TService), instance);
    }

    public static void AddDependencyInstance<TService>(this IDependencyResolver resolver, object instance,
      DependencyLifetime lifetime)
    {
      resolver.AddDependencyInstance(typeof(TService), instance, lifetime);
    }

    public static bool HasDependency<T>(this IDependencyResolver resolver)
    {
      return resolver.HasDependency(typeof(T));
    }

    /// <summary>
    /// Returns an instance of a registered dependency of type T.
    /// </summary>
    /// <typeparam name="T">The dependency type.</typeparam>
    /// <param name="resolver"></param>
    /// <returns>An instance of T.</returns>
    /// <exception cref="DependencyResolutionException">The resolver couldn't resolve the exception.</exception>
    public static T Resolve<T>(this IDependencyResolver resolver)
      where T : class
    {
      var untyped = resolver.Resolve(typeof(T));
      return (T) untyped;
    }

    public static T Resolve<T>(this IDependencyResolver resolver, UnregisteredAction unregistered)
      where T : class
    {
      return (T) resolver.Resolve(typeof(T), unregistered);
    }

    public static object Resolve(this IDependencyResolver resolver, Type type, UnregisteredAction unregisteredBehavior)
    {
      if (unregisteredBehavior == UnregisteredAction.AddAsTransient && !resolver.HasDependency(type))
        resolver.AddDependency(type, DependencyLifetime.Transient);
      return resolver.Resolve(type);
    }

    public static T ResolveWithDefault<T>(this IDependencyResolver resolver, Func<T> defaultValue)
      where T : class
    {
      return resolver.HasDependency(typeof(T)) ? resolver.Resolve<T>() : defaultValue();
    }
  }
}