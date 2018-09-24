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

namespace OpenRastaDemo.Benchmark
{
  [CoreJob]
  public class HydraBenchmark
  {
    private TestServer server;

    private HttpClient client;

    [GlobalSetup]
    public void Setup()
    {
      var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "large.json"));

      DemoHydraResponse.LargeJson = JsonConvert.DeserializeObject<List<HydraRootResponse>>(json);

      this.server = new TestServer(new WebHostBuilder()
        .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(new DemoConfigurationSource()))
        .UseStartup<Startup>()
      );

      this.client = this.server.CreateClient();
      this.client.DefaultRequestHeaders.Add("Accept", "application/ld+json");
    }

    [Benchmark]
    public async Task<HttpResponseMessage> GetMeSomeHydra()
    {
      return await this.client.GetAsync("/hydra");
    }

    [Benchmark]
    public async Task<HttpResponseMessage> GetMeSomeLittleHydra()
    {
      return await this.client.GetAsync("/littlehydra");
    }
  }
}