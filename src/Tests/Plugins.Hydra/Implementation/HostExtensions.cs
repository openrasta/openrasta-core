using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenRasta.Hosting.InMemory;

namespace Tests.Plugins.Hydra.Implementation
{
  public static  class HostExtensions
  {
    public static async Task<JToken> GetJsonLd(this InMemoryHost host, string uri)
    {
      var response = await host.Get(uri, "application/ld+json");
      if (response.StatusCode / 100 != 2)
        throw new InvalidOperationException($"Returned a {response.StatusCode} status code");
      var responseBody = response.ReadString();
      return JObject.Parse(responseBody);
    }
  }
}