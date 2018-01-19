using System;
using System.Threading;
using OpenRasta.Configuration.Fluent.Implementation;

namespace OpenRasta.Configuration.Fluent.Internal
{
  static class AsyncLocalConfigurations
  {
    static readonly AsyncLocal<FluentTarget> _target = new AsyncLocal<FluentTarget>();
    static readonly AsyncLocal<Action> _completion = new AsyncLocal<Action>();
    public static FluentTarget Target
    {
      get => _target.Value ?? throw new InvalidOperationException("Configuration attempted outside of a configuration class");
      set => _target.Value = value;
    }
    public static Action ConfigurationCompletion
    {
      get => _completion.Value ?? (() => { });
      set => _completion.Value = value;
    }
  }
}