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
  public class JsonBenchmark
  {
    private TestServer server;

    private HttpClient client;

    [GlobalSetup]
    public void Setup()
    {
      var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "large.json"));

      DemoJsonResponse.LargeJson = JsonConvert.DeserializeObject<IList<RootResponse>>(json);

      this.server = new TestServer(new WebHostBuilder()
        .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(new HydraApi(false,json)))
        .UseStartup<Startup>()
      );

      this.client = this.server.CreateClient();
      this.client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    [Benchmark]
    public async Task<HttpResponseMessage> GetMeSomeLittleJson()
    {
      return await this.client.GetAsync("/littlejson");
    }
  }
}