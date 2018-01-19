using System;
using System.Collections.Generic;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Tests.Unit.Infrastructure;

namespace Tests.Configuration.contexts
{
  public abstract class configuration : context, IDisposable
  {
    InMemoryHost _host;
    readonly List<Action> _uses = new List<Action>();

    readonly List<Action> _has = new List<Action>();

    protected void given_has<T>(Action<IResourceDefinition<T>> conf)
    {
      _has.Add(() => conf(ResourceSpace.Has.ResourcesOfType<T>()));
    }

    protected void given_has(Action<IHas> conf)
    {
      _has.Add(() => conf(ResourceSpace.Has));
    }

    protected void when_configured()
    {
      _host = new InMemoryHost(() =>
      {
        _uses.ForEach(_ => _());
        _has.ForEach(_ => _());
      });

      DependencyManager.SetResolver(_host.Resolver);
    }

    protected IMetaModelRepository Config => DependencyManager.GetService<IMetaModelRepository>();

    public void Dispose()
    {
      _host.Close();
      DependencyManager.UnsetResolver();
    }
  }
}