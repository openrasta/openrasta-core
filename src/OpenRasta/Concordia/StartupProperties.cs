using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection.Configuration;
using OpenRasta.Pipeline;

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
      }

      public PipelineProperties Pipeline { get; private set; }
    }

    public class PipelineProperties : AbstractProperties
    {
      public PipelineProperties(IDictionary<string, object> startupProperties)
        : base(startupProperties)
      {
      }

      public bool Validate
      {
        get { return Get("openrasta.pipeline.validate", true); }
        set { Set("openrasta.pipeline.validate", value); }
      }

      public IDictionary<
        Func<ContributorCall,bool>,
        Func<IPipelineMiddlewareFactory,IPipelineMiddlewareFactory>> MiddlewareInterceptors
      {
        get { return Get("openrasta.pipeline.yields", new Dictionary<Func<ContributorCall, bool>, Func<IPipelineMiddlewareFactory, IPipelineMiddlewareFactory>>()); }
        set { Set("openrasta.pipeline.validate", value); }
      }
    }
  }
}