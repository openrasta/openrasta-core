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
  public class JsonBenchmark
  {
    HttpClient client;
    TestServer server;

    [GlobalSetup]
    public void Setup()
    {
      var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "large.json"));

      DemoJsonResponse.LargeJson = JsonConvert.DeserializeObject<IList<RootResponse>>(json);

      server = new TestServer(new WebHostBuilder()
        .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(new DemoConfigurationSource()))
        .UseStartup<Startup>()
      );

      client = server.CreateClient();
      client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    [Benchmark]
    public async Task<HttpResponseMessage> GetMeSomeJson() => await client.GetAsync("/");

    [Benchmark]
    public async Task<HttpResponseMessage> GetMeSomeLittleJson() => await client.GetAsync("/littlejson");
  }
}