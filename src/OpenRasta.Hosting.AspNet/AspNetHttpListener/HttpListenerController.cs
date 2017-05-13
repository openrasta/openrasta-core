using System;
using System.Web.Hosting;

namespace OpenRasta.Hosting.AspNet.AspNetHttpListener
{
  // code originally based on xml-rpc.net under MIT license
  // see http://code.google.com/p/xmlrpcnet/
  public class HttpListenerController
  {
    readonly string _physicalDir;
    readonly Action _configuration;
    readonly string[] _prefixes;
    readonly string _virtualDir;

    public HttpListenerController(string[] prefixes, string vdir, string pdir, Action configuration)
    {
      _prefixes = prefixes;
      _virtualDir = vdir;
      _physicalDir = pdir;
      _configuration = configuration;
    }

    public HttpListenerAspNetHost Host { get; private set; }


    public void Start()
    {
      Host = (HttpListenerAspNetHost)ApplicationHost.CreateApplicationHost(
                                          typeof(HttpListenerAspNetHost), _virtualDir, _physicalDir);

      Host.Configure(_prefixes, _virtualDir, _physicalDir, _configuration);
      Host.Start();
    }

    public void Stop()
    {
      Host.Stop();
    }
  }
}