using System;
using System.Collections.Generic;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;

namespace OpenRasta.Tests.Unit.Configuration.contexts
{
    public abstract class configuration : IDisposable
    {

        IDisposable configCookie;
        InMemoryHost Host;
        List<Action<IHas>> _configActions = new List<Action<IHas>>();

        public configuration()
        {

            Host = new InMemoryHost(null);

            DependencyManager.SetResolver(Host.Resolver);
            configCookie = OpenRastaConfiguration.Manual;
        }
        protected void when_configuration_completed()
        {
                _configActions.ForEach(x => x(ResourceSpace.Has));
                if (configCookie != null)
                    configCookie.Dispose();
        }
        protected virtual void given_has(Action<IHas> has)
        {
            _configActions.Add(has);

        }
        public void Dispose()
        {
            configCookie = null;
            Host.Close();
        }
        protected IMetaModelRepository Configuration { get { return DependencyManager.GetService<IMetaModelRepository>(); } }
        protected class Resource { }
        protected class Handler {}
    }
}