using System;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Plugins.ReverseProxy
{
  static class ReverseProxyConstants
  {
    public const string ProxyTarget = "openrasta.ReverseProxy.Target";

    public static string GetReverseProxyTarget(this ResourceModel resource)
    {
      return (string)resource.Properties[ProxyTarget];
    }
    
    public static void ReverseProxyTarget(this ResourceModel resource, string target)
    {
      resource.Properties[ProxyTarget] = target;
    }
  }
}