using System;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Plugins.ReverseProxy
{
  static class ReverseProxyConstants
  {
    public const string ProxyTarget = "openrasta.ReverseProxy.Target";

    public static bool TryGetReverseProxyTarget(this ResourceModel resource, out Uri target)
    {
      if (resource.Properties.TryGetValue(ProxyTarget, out var entry) && entry is Uri entryUri)
      {
        target = entryUri;
        return true;
      }

      target = null;
      return false;
    }

    public static Uri GetReverseProxyTarget(this ResourceModel resource)
    {
      return (Uri)resource.Properties[ProxyTarget];
    }
    public static void ReverseProxyTarget(this ResourceModel resource, Uri target)
    {
      resource.Properties[ProxyTarget] = target;
    }
  }
}