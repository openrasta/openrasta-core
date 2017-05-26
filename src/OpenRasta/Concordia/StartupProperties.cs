using System;
using System.Collections.Generic;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.CallGraph;

// ReSharper disable MemberCanBePrivate.Global

namespace OpenRasta.Concordia
{

  public abstract class AbstractProperties
  {
    public IDictionary<string, object> Properties { get; }

    protected AbstractProperties(IDictionary<string, object> properties)
    {
      Properties = properties;
    }

    protected T Get<T>(string key, T defaultValue = default(T))
    {
      if (Properties.ContainsKey(key) == false) return (T)(Properties[key] = defaultValue);
      return (T) Properties[key];
    }

    protected T GetOrAdd<T>(string key, Func<T> add)
    {
      if (Properties.ContainsKey(key) == false) return (T)(Properties[key] = add());
      return (T) Properties[key];
    }
    protected Func<T> Get<T>(string key, Func<T> defaultValue)
    {
      if (Properties.TryGetValue(key, out var value) == false)
        value = Properties[key] = defaultValue;
      return (Func<T>) value;
    }
    protected void Set<T>(string key, T value)
    {
      Properties[key] = value;
    }
  }

  public class StartupProperties : AbstractProperties
  {
    public StartupProperties()
      : this(new Dictionary<string, object>())
    {
    }

    StartupProperties(IDictionary<string, object> startupProperties)
      : base(startupProperties)
    {
      OpenRasta = new OpenRastaProperties(startupProperties);
    }

    public OpenRastaProperties OpenRasta { get; }

    public class OpenRastaProperties : AbstractProperties
    {
      public OpenRastaProperties(IDictionary<string, object> startupProperties)
        : base(startupProperties)
      {
        Errors = new ErrorProperties(startupProperties);
        Diagnostics = new DiagnosticsProperties(startupProperties);
        Factories = new FactoriesProperties(startupProperties);
        Pipeline = new PipelineProperties(startupProperties);
      }

      public FactoriesProperties Factories { get; }

      public DiagnosticsProperties Diagnostics { get; }

      public PipelineProperties Pipeline { get; }
      public ErrorProperties Errors { get; }
    }

    public class FactoriesProperties : AbstractProperties
    {
      public FactoriesProperties(IDictionary<string, object> properties) : base(properties)
      {
      }

      public IDependencyResolver Resolver
      {
        get => GetOrAdd("openrasta.factories.DependencyResolver", ()=> new InternalDependencyResolver());
        set => Set("openrasta.factories.DependencyResolver", value);
      }

      public Func<IEnumerable<IPipelineContributor>> Contributors
      {
        get => Get("openrasta.factories.Contributors", ()=> Resolver.ResolveAll<IPipelineContributor>());
        set => Set("openrasta.factories.Contributors", value);
      }
      
      public Func<IGenerateCallGraphs> CallGraphGenerator
      {
        get => Get("openrasta.factories.CallGraphGenerator", () =>
          Resolver.HasDependency<IGenerateCallGraphs>()
            ? Resolver.Resolve<IGenerateCallGraphs>()
            : new WeightedCallGraphGenerator());
        set => Set("openrasta.factories.CallGraphGenerator", value);
      }
    }
    public class DiagnosticsProperties : AbstractProperties
    {
      public DiagnosticsProperties(IDictionary<string, object> properties) : base(properties)
      {
      }

      public bool TracePipelineExecution
      {
        get => Get("openrasta.diagnostics.TracePipelineExecution", true);
        set => Set("openrasta.diagnostics.TracePipelineExecution", value);
      }
    }
    public class PipelineProperties : AbstractProperties
    {
      public PipelineProperties(IDictionary<string, object> properties) : base(properties)
      {
      }

      public bool Validate
      {
        get => Get("openrasta.pipeline.validate", true);
        set => Set("openrasta.pipeline.validate", value);
      }

      public IDictionary<
        Func<ContributorCall,bool>,
        Func<IPipelineMiddlewareFactory>> ContributorTrailers
      {
        get => Get("openrasta.pipeline.trailers", new Dictionary<Func<ContributorCall, bool>, Func<IPipelineMiddlewareFactory>>());
        set => Set("openrasta.pipeline.trailers", value);
      }
    }

    public class ErrorProperties : AbstractProperties
    {
      public ErrorProperties(IDictionary<string, object> properties) : base(properties)
      {
      }

      public bool HandleCatastrophicExceptions
      {
        get => Get(Keys.HandleCatastrophicExceptions, true);
        set => Set(Keys.HandleCatastrophicExceptions, value);
      }

      public bool HandleAllExceptions {
        get => Get(Keys.HandleAllExceptions, true);
        set => Set(Keys.HandleAllExceptions, value);}
    }
  }
}