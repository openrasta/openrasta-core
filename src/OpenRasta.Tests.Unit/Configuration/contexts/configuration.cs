using System;
using System.Collections.Generic;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Testing;
using OpenRasta.TypeSystem;

namespace Tests.Configuration.contexts
{
    public abstract class configuration : context, IDisposable
    {
        InMemoryHost Host;
        List<Action> _uses = new List<Action>();

        List<Action> _has = new List<Action>();
        
        protected virtual void given_uses(Action<IUses> conf)
        {
            _uses.Add(() => conf(ResourceSpace.Uses));
        }
        protected virtual void given_has<T>(Action<IResourceDefinition<T>> conf)
        {
            _has.Add(() => conf(ResourceSpace.Has.ResourcesOfType<T>()));
        }
        protected virtual void given_has(Action<IHas> conf)
        {
            _has.Add(() => conf(ResourceSpace.Has));
        }
        protected virtual void when_configured()
        {
            Host = new InMemoryHost(null);

            DependencyManager.SetResolver(Host.Resolver);
            using (OpenRastaConfiguration.Manual) 
            {
                _uses.ForEach(_ => _());
                _has.ForEach(_ => _());
            }
        }
        protected ITypeSystem TypeSystem { get { return DependencyManager.GetService<ITypeSystem>(); } }
        protected IMetaModelRepository Config { get { return DependencyManager.GetService<IMetaModelRepository>(); } }
        protected class Customer { }
        protected class CustomerHandler { }

        public void Dispose()
        {
            Host.Close();
            DependencyManager.UnsetResolver();
        }
    }
}