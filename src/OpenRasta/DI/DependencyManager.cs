using System;
using System.Threading;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.Handlers;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.DI
{
  /// <summary>
  /// Provides easy access to common services and dependency-specific properties.
  /// </summary>
  public static class DependencyManager
  {
    static readonly AsyncLocal<IDependencyResolver> _current = new AsyncLocal<IDependencyResolver>();
    public static IDependencyResolver Current
    {
      get => _current.Value;
      private set => _current.Value = value;
    }

    static DependencyManager()
    {
      AutoRegisterDependencies = true;
    }

    /// <summary>
    /// Gets or sets a value defining if unregistered dependencies resolved through a call to <see cref="GetService"/>
    /// are automatically registered in the container.
    /// </summary>
    /// <remarks>This covers user-specified codecs, handlers and any type provided to the <see cref="GetService"/> method.
    /// <c>true</c> by default.</remarks>
    public static bool AutoRegisterDependencies { get; set; }

    public static ICodecRepository Codecs => GetService<ICodecRepository>();

    public static IHandlerRepository Handlers => GetService<IHandlerRepository>();

    public static bool IsAvailable => Current != null;

    public static IPipeline Pipeline => GetService<IPipeline>();

    public static IUriResolver Uris => GetService<IUriResolver>();

    public static T GetService<T>() where T : class
    {
      return (T) GetService(typeof(T));
    }

    /// <summary>
    /// Resolve a component, optionally registering it in the container if <see cref="AutoRegisterDependencies"/> is set to <c>true</c>.
    /// </summary>
    /// <param name="dependencyType"></param>
    /// <returns></returns>
    public static object GetService(Type dependencyType)
    {
      if (dependencyType == null)
        return null;
      if (Current == null)
        throw new DependencyResolutionException("Cannot resolve services when no _resolver has been configured.");
      if (AutoRegisterDependencies && !dependencyType.IsAbstract)
      {
        if (!Current.HasDependency(dependencyType))
          Current.AddDependency(dependencyType, DependencyLifetime.Transient);
      }
      return Current.Resolve(dependencyType);
    }

    /// <summary>
    /// Set a dependency resolver for the current thread
    /// </summary>
    /// <param name="resolver">An instance of a dependency resolver.</param>
    /// <remarks>If no dependency registrar is registered in the container, the <see cref="DefaultDependencyRegistrar"/> will be used instead.</remarks>
    public static void SetResolver(IDependencyResolver resolver)
    {
      if (Current != null)
        throw new InvalidOperationException("A resolver is already set");
      Current = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    public static void UnsetResolver()
    {
      if (Current != null) Current = null;
    }
  }
}