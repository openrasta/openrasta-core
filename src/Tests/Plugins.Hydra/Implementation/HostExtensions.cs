using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;

namespace Tests.Plugins.Hydra.Implementation
{
  public static  class HostExtensions
  {
    public static async Task<JToken> GetJsonLdContent(this InMemoryHost host, string uri)
    {
      var response = await host.Get(uri,"application/ld+json");
      if (response.StatusCode / 100 != 2)
        throw new InvalidOperationException($"Returned a {response.StatusCode} status code");
      var responseBody = response.ReadString();
      return JObject.Parse(responseBody,new JsonLoadSettings(){DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error});
    }
    
    public static async Task<string> GetJsonLdString(this InMemoryHost host, string uri)
    {
      var response = await host.Get(uri, "application/ld+json");
      if (response.StatusCode / 100 != 2)
        throw new InvalidOperationException($"Returned a {response.StatusCode} status code");
      return response.ReadString();
    }
    
    public static async Task<(IResponse,JToken)> GetJsonLd(this InMemoryHost host, string uri)
    {
      var response = await host.Get(uri, "application/ld+json");
      if (response.StatusCode / 100 != 2)
        throw new InvalidOperationException($"Returned a {response.StatusCode} status code");
      var responseBody = response.ReadString();
      Console.WriteLine(responseBody);
      return (response,JObject.Parse(responseBody, new JsonLoadSettings(){DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error}));
    }
    
    public static async Task<(IResponse, JToken)> PostJsonLd(this InMemoryHost host, string uri, string content)
    {
      var response = await host.Post(uri, content, contentType: "application/ld+json", accept: "application/ld+json");
      if (response.StatusCode / 100 != 2)
        throw new InvalidOperationException($"Returned a {response.StatusCode} status code");
      var responseBody = response.ReadString();
      return (response,JObject.Parse(responseBody,new JsonLoadSettings(){DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error}));
    }
  }
}