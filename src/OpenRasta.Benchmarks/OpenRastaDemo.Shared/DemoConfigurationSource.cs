using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.ReverseProxy;

namespace OpenRastaDemo.Shared
{
  public class DemoConfigurationSource : IConfigurationSource
  {
    public int ResponseCount { get; }
    readonly string _reverseProxyUri;

    public DemoConfigurationSource(int responseCount, string reverseProxyUri = null)
    {
      ResponseCount = responseCount;
      ReverseProxyOptions = new ReverseProxyOptions();

      _reverseProxyUri = reverseProxyUri ?? "http://localhost/demoreverseproxy";
    }

    public ReverseProxyOptions ReverseProxyOptions { get; }

    public void Configure()
    {
      ResourceSpace.Uses
        .Hydra(options => options.Vocabulary = "https://schemas.example/schema#")
        .ReverseProxy(ReverseProxyOptions);

      ResourceSpace.Has
        .ResourcesOfType<IEnumerable<RootResponse>>()
        .AtUri("/")
        .Named("root")
        .HandledBy<RootHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>()
        .ForMediaType("application/json");

      ResourceSpace.Has
        .ResourcesOfType<RootResponse>()
        .AtUri("/littlejson")
        .Named("littlejson")
        .HandledBy<LittleJsonHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>()
        .ForMediaType("application/json");

      ResourceSpace.Uses.Dependency(ctx => ctx.Singleton(() => new HydraRootHandler(
        GenerateHydraRootResponses(ResponseCount))).As<HydraRootHandler>());
      
      ResourceSpace.Has
        .ResourcesOfType<List<HydraRootResponse>>()
        .AtUri("/rootResponses")
        .EntryPointCollection()
        .HandledBy<HydraRootHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>()
        .ForMediaType("application/json");

      ResourceSpace.Has
        .ResourcesNamed("reverseproxy")
        .AtUri("/reverseproxy")
        .ReverseProxyFor(_reverseProxyUri);

      ResourceSpace.Has
        .ResourcesNamed("demoreverseproxy")
        .AtUri("/demoreverseproxy")
        .HandledBy<DemoReverseProxyHandler>();
    }

    List<HydraRootResponse> GenerateHydraRootResponses(int responseCount)
    {
      return Enumerable.Repeat(
        new HydraRootResponse
        {
          _id = "1",
          about = "about",
          address = "10 downing street",
          age = 21,
          balance = "12.45",
          company = "IBM",
          email = "me@home.com",
          friends = new List<Friend>(new[] {new Friend {id = 2, name = "Joey"}}),
          name = "Bob",
          gender = "male",
          greeting = "hi",
          guid = Guid.Empty,
          index = 4,
          latitude = 56.54,
          longitude = Decimal.One,
          phone = "020 7890 4322",
          picture = new Uri("http://www.google.com"),
          registered = DateTime.UtcNow,
          tags = new List<string>(new[] {"awesome"}),
          eyeColor = "green",
          favoriteFruit = "coconut",
          isActive = true
        }, responseCount).ToList();
    }
  }
}