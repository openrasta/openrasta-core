using System;
using OpenRasta.Codecs;
using OpenRasta.DI;
using OpenRasta.Hosting;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;

namespace OpenRasta.Testing.Contexts
{
    public abstract class codec_context<TCodec> : context where TCodec:ICodec
    {
      private IDisposable _rqx;
      public InMemoryHost Host { get; private set; }
        protected ICommunicationContext Context { get; private set; }
        protected HostManager HostManager { get; set; }
        protected abstract TCodec CreateCodec(ICommunicationContext context);

        protected void given_context()
        {
            Host = new InMemoryHost(null);
            HostManager = Host.HostManager;
            _rqx = Host.Resolver.CreateRequestScope();
            HostManager.SetupCommunicationContext(Context = new InMemoryCommunicationContext());
            DependencyManager.SetResolver(Host.Resolver);
        }
        
        protected override void TearDown()
        {
          _rqx.Dispose();
            DependencyManager.UnsetResolver();
        }
    }
}