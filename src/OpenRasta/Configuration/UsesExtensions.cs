using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Web;
using OpenRasta.Web.UriDecorators;

namespace OpenRasta.Configuration
{
  public static class UsesExtensions
  {
    /// <summary>
    /// Registers a custom dependency that can be used for leveraging dependency injection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register</typeparam>
    /// <typeparam name="TConcrete">The concrete type used when the service type is requested</typeparam>
    /// <param name="anchor"></param>
    /// <param name="lifetime">The lifetime of the object.</param>
    public static void CustomDependency<TService, TConcrete>(
      this IUses anchor,
      DependencyLifetime lifetime = DependencyLifetime.Singleton) where TConcrete : TService
    {
      var fluentTarget = (IFluentTarget) anchor;

      fluentTarget.Repository.CustomRegistrations.Add(
        new DependencyRegistrationModel(typeof(TService), typeof(TConcrete), lifetime));
    }

    public static TAnchor Dependency<TAnchor>(this TAnchor anchor, Action<ITypeRegistrationContext> ctx)
      where TAnchor : IUses
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
      anchor.CustomDependency<IPipelineContributor, TPipeline>();
    }

    public static IUses PipelineContributor<TPipeline>(this IUses anchor, Expression<Func<TPipeline>> factory)
      where TPipeline : class, IPipelineContributor
    {
      anchor.Dependency(d => d.Singleton(factory).As<IPipelineContributor>());
      return anchor;
    }

    public static IUses PipelineContributor<TArg, TPipeline>(
      this IUses anchor,
      Expression<Func<TArg, TPipeline>> factory) where TPipeline : class, IPipelineContributor
    {
      anchor.Dependency(d => d.Singleton(factory).As<IPipelineContributor>());
      return anchor;
    }

    public static IUses PipelineContributor<TArg1, TArg2, TPipeline>(
      this IUses anchor,
      Expression<Func<TArg1, TArg2, TPipeline>> factory) where TPipeline : class, IPipelineContributor
    {
      anchor.Dependency(d => d.Singleton(factory).As<IPipelineContributor>());
      return anchor;
    }

    public static IUses PipelineContributor<TArg1, TArg2, TArg3, TPipeline>(
      this IUses anchor,
      Expression<Func<TArg1, TArg2, TArg3, TPipeline>> factory) where TPipeline : class, IPipelineContributor
    {
      anchor.Dependency(d => d.Singleton(factory).As<IPipelineContributor>());
      return anchor;
    }

    public static IUses PipelineContributor<TArg1, TArg2, TArg3, TArg4, TPipeline>(
      this IUses anchor,
      Expression<Func<TArg1, TArg2, TArg3, TArg4, TPipeline>> factory) where TPipeline : class, IPipelineContributor
    {
      anchor.Dependency(d => d.Singleton(factory).As<IPipelineContributor>());
      return anchor;
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