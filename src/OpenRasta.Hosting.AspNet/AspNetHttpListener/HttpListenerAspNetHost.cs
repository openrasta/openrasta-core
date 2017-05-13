using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using OpenRasta.Configuration;

namespace OpenRasta.Hosting.AspNet.AspNetHttpListener
{
// Warning, this class will undergo massive refactorings sooner or later, don't rely on it.
  public class HttpListenerAspNetHost : MarshalByRefObject
  {
    System.Net.HttpListener _listener;
    string _physicalDir;
    string _virtualDir;

    public void Configure(string[] prefixes, string vdir, string pdir, Action configuration)
    {
      _virtualDir = vdir;
      _physicalDir = pdir;
      _listener = new System.Net.HttpListener();
      foreach (var prefix in prefixes)
        _listener.Prefixes.Add(prefix);
      AspNetHost.ConfigurationSourceLocator = () => new DelegateConfiguration(configuration);
    }

    public override object InitializeLifetimeService()
    {
      return null;
    }

    public void ProcessRequest()
    {
      HttpListenerContext ctx;
      try
      {
        ctx = _listener.GetContext();
      }
      catch (HttpListenerException)
      {
        return;
      }
      QueueNextRequestWait();
      var workerRequest = new HttpListenerWorkerRequest(ctx, _virtualDir, _physicalDir);
      try
      {
        HttpRuntime.ProcessRequest(workerRequest);
      }
      catch (Exception e)
      {
        Trace.WriteLine(e.ToString());
      }
    }

    public void Start()
    {
      var list = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList();
      foreach (var asm in list)
      {
        Trace.WriteLine(asm.FullName);
      }

      _listener.Start();
      QueueNextRequestWait();
    }

    public void Stop()
    {
      _listener.Stop();
      ApplicationManager.GetApplicationManager().ShutdownAll();
    }

    void QueueNextRequestWait()
    {
      ThreadPool.QueueUserWorkItem(s => ProcessRequest());
    }

    class DelegateConfiguration : IConfigurationSource
    {
      Action _configuration;

      public DelegateConfiguration(Action configuration)
      {
        _configuration = configuration;
      }

      public void Configure()
      {
        _configuration();
      }
    }
  }
}