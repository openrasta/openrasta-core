using System;
using System.Collections.Generic;
using OpenRasta.Pipeline;
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

    protected T Get<T>(string key, T defaultValue = default)
    {

      if (Properties.ContainsKey(key) == false) return (T)(Properties[key] = defaultValue);
      return (T) Properties[key];
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
        Pipeline = new PipelineProperties(startupProperties);
        Errors = new ErrorProperties(startupProperties);
        Diagnostics = new DiagnosticsProperties(startupProperties);
      }

      public DiagnosticsProperties Diagnostics { get; }

      public PipelineProperties Pipeline { get; }
      public ErrorProperties Errors { get; }
    }

    public class DiagnosticsProperties : AbstractProperties
    {
      public DiagnosticsProperties(IDictionary<string, object> properties) : base(properties)
      {
      }

      public bool TracePipelineExecution
      {
        get => Get("openrasta.diagnostics.TracePipelineExecution", false);
        set => Set("openrasta.diagnostics.TracePipelineExecution", value);
      }
    }
    public class PipelineProperties : AbstractProperties
    {
      public PipelineProperties(IDictionary<string, object> startupProperties)
        : base(startupProperties)
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