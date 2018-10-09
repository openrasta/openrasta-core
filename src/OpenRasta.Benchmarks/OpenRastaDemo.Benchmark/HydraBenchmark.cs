using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRastaDemo.Shared;

namespace OpenRastaDemo.Benchmark
{
  [CoreJob]
  public class HydraBenchmark
  {
    HttpClient client;
    TestServer server;

    [GlobalSetup]
    public void Setup()
    {
      var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "large.json"));

      DemoHydraResponse.LargeJson = JsonConvert.DeserializeObject<List<HydraRootResponse>>(json);

      server = new TestServer(new WebHostBuilder()
        .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(new DemoConfigurationSource()))
        .UseStartup<Startup>()
      );

      client = server.CreateClient();
      client.DefaultRequestHeaders.Add("Accept", "application/ld+json");
    }

    [Benchmark]
    public async Task<HttpResponseMessage> GetMeSomeHydra() => await client.GetAsync("/hydra");

    [Benchmark]
    public async Task<HttpResponseMessage> GetMeSomeLittleHydra() => await client.GetAsync("/littlehydra");
  }
}