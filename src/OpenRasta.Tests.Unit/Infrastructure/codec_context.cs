using System;
using System.Threading;
using OpenRasta.Codecs;
using OpenRasta.DI;
using OpenRasta.Hosting;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;

namespace OpenRasta.Tests.Unit.Infrastructure
{
  public abstract class codec_context<TCodec> : context where TCodec : ICodec
  {
    public InMemoryHost Host { get; private set; }
    protected ICommunicationContext Context { get; private set; }
    protected HostManager HostManager { get; set; }
    protected abstract TCodec CreateCodec(ICommunicationContext context);

    protected void given_context()
    {
      Host = new InMemoryHost();
      HostManager = Host.HostManager;
      DependencyManager.SetResolver(Host.Resolver);
      RequestScope = Host.Resolver.CreateRequestScope();
      HostManager.SetupCommunicationContext(Context = new InMemoryCommunicationContext());
    }

    IDisposable RequestScope { get; set; }

    protected override void TearDown()
    {
      RequestScope.Dispose();
      DependencyManager.UnsetResolver();
    }
  }
}