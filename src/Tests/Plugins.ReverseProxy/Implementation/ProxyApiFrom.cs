using OpenRasta.Configuration;
using OpenRasta.Plugins.ReverseProxy;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxyApiFrom : IConfigurationSource
  {
    readonly string from;
    readonly string to;
    readonly ReverseProxyOptions options;

    public ProxyApiFrom(
        string from,
        string to,
        ReverseProxyOptions options)
    {
      this.from = from;
      this.to = to;
      this.options = options;
    }

    public void Configure()
    {
      ResourceSpace.Has
          .ResourcesNamed("from")
          .AtUri(from)
          .ReverseProxyFor($"http://destination.example{to}");

      ResourceSpace.Uses.ReverseProxy(options);
    }
  }
}