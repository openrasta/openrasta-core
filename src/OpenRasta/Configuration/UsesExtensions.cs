using System;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Web.UriDecorators;

namespace OpenRasta.Configuration
{
  public interface ITypeRegistrationContext
  {
//    ITypeRegistrationOptions Register<TConcrete>();
    ITypeRegistrationOptions Register<TConcrete>(Func<TConcrete> factory);
//    ITypeRegistrationOptions Register<TArg1, TConcrete>(Func<TArg1, TConcrete> factory);
//    ITypeRegistrationOptions Register<TArg1, TArg2, TConcrete>(Func<TArg1, TArg2, TConcrete> factory);
//    ITypeRegistrationOptions Register<TArg1, TArg2, TArg3, TConcrete>(Func<TArg1, TArg2, TArg3, TConcrete> factory);
//
//    ITypeRegistrationOptions Register<TArg1, TArg2, TArg3, TArg4, TConcrete>(
//      Func<TArg1, TArg2, TArg3, TArg4, TConcrete> factory);
//
//    ITypeRegistrationOptions Register<TArg1, TArg2, TArg3, TArg4, TArg5, TConcrete>(
//      Func<TArg1, TArg2, TArg3, TArg4, TArg5, TConcrete> factory);
  }

  public interface ITypeRegistrationOptions
  {
    ITypeRegistrationOptions As<T>();
    ITypeRegistrationOptions Lifetime(DependencyLifetime lifetime = DependencyLifetime.Transient);
  }

  class TypeRegistrationContext : ITypeRegistrationContext, ITypeRegistrationOptions
  {
    public ITypeRegistrationOptions Register<TConcrete>(Func<TConcrete> factory)
    {
      Model = new DependencyFactoryModel<TConcrete>(factory);
      return this;
    }

    public DependencyFactoryModel Model { get; set; }
    public ITypeRegistrationOptions As<T>()
    {
      Model.ServiceType = typeof(T);
      return this;
    }

    public ITypeRegistrationOptions Lifetime(DependencyLifetime lifetime)
    {
      Model.Lifetime = lifetime;
      return this;
    }
  }
  public static class UsesExtensions
  {
    /// <summary>
    /// Registers a custom dependency that can be used for leveraging dependency injection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register</typeparam>
    /// <typeparam name="TConcrete">The concrete type used when the service type is requested</typeparam>
    /// <param name="anchor"></param>
    /// <param name="lifetime">The lifetime of the object.</param>
    public static void CustomDependency<TService, TConcrete>(this IUses anchor,
      DependencyLifetime lifetime = DependencyLifetime.Singleton) where TConcrete : TService
    {
      var fluentTarget = (IFluentTarget) anchor;

      fluentTarget.Repository.CustomRegistrations.Add(
        new DependencyRegistrationModel(typeof(TService), typeof(TConcrete), lifetime));
    }

    public static TAnchor Dependency<TAnchor>(this TAnchor anchor, Action<ITypeRegistrationContext> ctx) where TAnchor : IUses
    {
      var fluentTarget = (IFluentTarget) anchor;
      var typeRegistration = new TypeRegistrationContext();
      ctx(typeRegistration);
      fluentTarget.Repository.CustomRegistrations.Add(typeRegistration.Model);

      return anchor;
    }

    /// <summary>
    /// Adds a contributor to the pipeline.
    /// </summary>
    /// <typeparam name="TPipeline">The type of the pipeline contributor to register.</typeparam>
    /// <param name="anchor"></param>
    public static void PipelineContributor<TPipeline>(this IUses anchor) where TPipeline : class, IPipelineContributor
    {
      anchor.Resolver.AddDependency<IPipelineContributor, TPipeline>();
    }

    /// <summary>
    /// Adds a URI decorator to process incoming URIs.
    /// </summary>
    /// <typeparam name="TDecorator">The type of the URI decorator.</typeparam>
    /// <param name="anchor"></param>
    public static void UriDecorator<TDecorator>(this IUses anchor) where TDecorator : class, IUriDecorator
    {
      var fluentTarget = (IFluentTarget) anchor;

      fluentTarget.Repository.CustomRegistrations.Add(
        new DependencyRegistrationModel(typeof(IUriDecorator),
        typeof(TDecorator), DependencyLifetime.Transient));
    }
  }
}