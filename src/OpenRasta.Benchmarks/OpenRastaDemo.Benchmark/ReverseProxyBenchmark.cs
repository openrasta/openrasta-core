using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OpenRasta.Configuration;
using OpenRastaDemo.Shared;

namespace OpenRastaDemo.Benchmark
{
  [SimpleJob(RuntimeMoniker.Net48)]
  [SimpleJob(RuntimeMoniker.Net60)]
  public class ReverseProxyBenchmark
  {
    HttpClient client;
    TestServer server;

    [GlobalSetup]
    public void Setup()
    {
      var configurationSource = new DemoConfigurationSource(1);

      server = new TestServer(new WebHostBuilder()
        .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(configurationSource))
        .UseStartup<Startup>()
      );

      client = server.CreateClient();
      client.DefaultRequestHeaders.Add("Accept", "text/plain");

      configurationSource.ReverseProxyOptions.HttpClient.Handler = () => server.CreateHandler();
    }

    [Benchmark]
    public async Task<HttpResponseMessage> GetMeSomeProxy() => await client.GetAsync("/reverseproxy");
  }
}