using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Tests.Hosting.Owin
{
  public static class WebHostExtensions
  {
    public static int Port(this IWebHost host)
    {
      return host.ServerFeatures.Port();
    }
    // ReSharper disable once MemberCanBePrivate.Global
    public static int Port(this IFeatureCollection features)
    {
      return features
        .Get<IServerAddressesFeature>().Addresses
        .Select(a=>new Uri(a).Port)
        .Single();
    }
  }
}