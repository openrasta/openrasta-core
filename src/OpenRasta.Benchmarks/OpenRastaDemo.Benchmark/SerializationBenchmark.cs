using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Web.Markup.Modules;
using OpenRastaDemo.Shared;

namespace OpenRastaDemo.Benchmark
{
  [CoreJob(), MemoryDiagnoser,HtmlExporter(),GcServer(),ReturnValueValidator(true)]
  public class SerializationBenchmark
  {
    HttpClient client;
    TestServer server;

    [Params(50000)] public int Items;
    
    [GlobalSetup]
    public void Setup()
    {
      server = new TestServer(new WebHostBuilder()
        .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(new DemoConfigurationSource(Items)))
        .UseStartup<Startup>()
      );

      client = server.CreateClient();
      client.DefaultRequestHeaders.Add("Accept", "application/ld+json");
    }

    async Task<HttpResponseMessage> Load(string accept)
    {
      var result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/rootResponses")
      {
        Headers = {Accept = {new MediaTypeWithQualityHeaderValue(accept)}}
      });
      result.EnsureSuccessStatusCode();
      return result;
    }
    [Benchmark]
    public HttpResponseMessage GetHydra() => Load("application/ld+json").Result;

    [Benchmark(Baseline = true)]
    public HttpResponseMessage GetNewtonsoftJson() => Load("application/json").Result;

  }
}