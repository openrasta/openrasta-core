using System;
using System.Linq.Expressions;
using OpenRasta.Configuration.Fluent;
using OpenRasta.DI;
using OpenRasta.TypeSystem;
using OpenRasta.TypeSystem.ReflectionBased;

namespace Tests
{
  public static class HandlerFactoryExtensions
  {
    public static IHandlerForResourceWithUriDefinition HandledBy<THandler>(
      this IUriDefinition definition,
      Expression<Func<THandler>> factory)
    {
      var f = factory.Compile();
      return definition.HandledBy(ContainerTypeSystem<THandler>.Create((_ => f())));
    }

    public static IHandlerForResourceWithUriDefinition HandledBy<TArg, THandler>(
      this IUriDefinition definition,
      Expression<Func<TArg, THandler>> factory) where TArg : class
    {
      var f = factory.Compile();
      return definition.HandledBy(ContainerTypeSystem<THandler>.Create((
        resolver => f(resolver.Resolve<TArg>()))));
    }

    public static IHandlerForResourceWithUriDefinition HandledBy<TArg1, TArg2, THandler>(
      this IUriDefinition definition,
      Expression<Func<TArg1, TArg2, THandler>> handlerFactory) where TArg1 : class where TArg2 : class
    {
      var f = handlerFactory.Compile();
      return definition.HandledBy(ContainerTypeSystem<THandler>.Create(
        resolver => f(resolver.Resolve<TArg1>(), resolver.Resolve<TArg2>())));
    }

    public static IHandlerForResourceWithUriDefinition HandledBy<TArg1, TArg2, TArg3, THandler>(
      this IUriDefinition definition,
      Expression<Func<TArg1, TArg2, TArg3, THandler>> factory)
      where TArg1 : class where TArg2 : class where TArg3 : class
    {
      var f = factory.Compile();
      return definition.HandledBy(ContainerTypeSystem<THandler>.Create(
        resolver => f(
          resolver.Resolve<TArg1>(),
          resolver.Resolve<TArg2>(),
          resolver.Resolve<TArg3>()
        )));
    }

    public static IHandlerForResourceWithUriDefinition HandledBy<TArg1, TArg2, TArg3, TArg4, THandler>(
      this IUriDefinition definition,
      Expression<Func<TArg1, TArg2, TArg3, TArg4, THandler>> factory) where TArg1 : class
      where TArg2 : class
      where TArg3 : class
      where TArg4 : class
    {
      var f = factory.Compile();
      return definition.HandledBy(ContainerTypeSystem<THandler>.Create(
        resolver => f(
          resolver.Resolve<TArg1>(),
          resolver.Resolve<TArg2>(),
          resolver.Resolve<TArg3>(),
          resolver.Resolve<TArg4>()
        )));
    }

    public class ContainerTypeSystem<T> : ITypeSystem
    {
      readonly ITypeSystem _typeSystemImplementation = TypeSystems.Default;
      readonly HandlerFactoryType _factoryType;

      public static IType Create(Func<IDependencyResolver, T> factory)
      {
        return new ContainerTypeSystem<T>(factory).FromClr(typeof(T));
      }

      ContainerTypeSystem(Func<IDependencyResolver, T> factory)
      {
        _factoryType = new HandlerFactoryType(factory, this);
      }

      public IType FromClr(Type type)
      {
        return type == typeof(T) ? _factoryType : _typeSystemImplementation.FromClr(type);
      }

      public IType FromInstance(object instance)
      {
        return _typeSystemImplementation.FromInstance(instance);
      }

      public ISurrogateProvider SurrogateProvider => _typeSystemImplementation.SurrogateProvider;
      public IPathManager PathManager => _typeSystemImplementation.PathManager;

      public class HandlerFactoryType : ReflectionBasedType
      {
        readonly Func<IDependencyResolver, T> _factory;

        public HandlerFactoryType(Func<IDependencyResolver, T> factory, ITypeSystem typeSystem) :
          base(typeSystem, typeof(T))
        {
          _factory = factory;
        }

        public override object CreateInstance(IDependencyResolver resolver)
        {
          return _factory(resolver);
        }
      }
    }
  }
}