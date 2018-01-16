using System;
using System.Threading;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.Concordia;
using OpenRasta.DI;
using OpenRasta.Hosting;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;

namespace OpenRasta.Tests.Unit.Infrastructure
{
  public abstract class codec_context<TCodec> : context where TCodec : ICodec
  {
    protected InMemoryHost Host { get; set; }
    protected ICommunicationContext Context { get; private set; }
    HostManager HostManager { get; set; }
    protected abstract TCodec CreateCodec(ICommunicationContext context);

    [SetUp]
    public void setup()
    {
      
      Host = new InMemoryHost(startup: new StartupProperties
      {
        OpenRasta =
        {
          Errors =
          {
            HandleAllExceptions = false,
            HandleCatastrophicExceptions = false
          }
        }
      });
      HostManager = Host.HostManager;
      AmbientContext.Current = new AmbientContext();
      RequestScope = Host.Resolver.CreateRequestScope();
      HostManager.SetupCommunicationContext(Context = new InMemoryCommunicationContext());
    }
    protected void given_context()
    {
    }


    IDisposable RequestScope { get; set; }

    protected override void TearDown()
    {
      try
      {
        RequestScope?.Dispose();
        Host?.Close();
      }
      finally
      {
        AmbientContext.Current = null;
      }
    }
  }
}