using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Hosting.AspNetCore;

namespace OpenRastaDemo.Benchmark
{
  [CoreJob]
  [GcServer(true)]
  [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
  [CategoriesColumn]
  [MemoryDiagnoser]
  [InvocationCount(10)]
  public class HydraBenchmark
  {
    TestServer newtonsoftServer;

    HttpClient newtonsoftClient;
    TestServer utf8Server;
    HttpClient utf8Client;

    [GlobalSetup]
    public void Setup()
    {
      var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "large.json"));

      newtonsoftServer = new TestServer(new WebHostBuilder()
        .Configure(c => c.UseOpenRasta(new HydraApi(false, json)))
      );
      utf8Server = new TestServer(new WebHostBuilder()
        .Configure(c => c.UseOpenRasta(new HydraApi(true, json)))
      );

      newtonsoftClient = newtonsoftServer.CreateClient();

      utf8Client = utf8Server.CreateClient();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
      newtonsoftServer.Dispose();
      utf8Server.Dispose();
    }


    [BenchmarkCategory("large"), Benchmark(Baseline = true)]
    public async Task<HttpResponseMessage> LargeNewtonsoftRaw()
    {
      return await Get(newtonsoftClient, "/hydra", "application/json");
    }
//    [BenchmarkCategory("large"), Benchmark]
//    public async Task<HttpResponseMessage> LargeNewtonsoft()
//    {
//      return await Get(newtonsoftClient, "/hydra", "application/ld+json");
//    }

    [BenchmarkCategory("large"), Benchmark()]
    public async Task<HttpResponseMessage> LargeUtf8()
    {
      return await Get(utf8Client, "/hydra", "application/ld+json");
    }
    
    [BenchmarkCategory("large"), Benchmark()]
    public async Task<HttpResponseMessage> LargeUtf8FastUri()
    {
      return await Get(utf8Client, "/hydrafasturi", "application/ld+json");
    }
    
//
//    [BenchmarkCategory("small"), Benchmark(Baseline = true)]
//    public async Task<HttpResponseMessage> SmallNewtonsoftRaw()
//    {
//      return await Get(newtonsoftClient, "/littlehydra", "application/json");
//    }
//
//    [BenchmarkCategory("small"), Benchmark]
//    public async Task<HttpResponseMessage> SmallNewtonsoft()
//    {
//      return await Get(newtonsoftClient, "/littlehydra", "application/ld+json");
//    }
//
//    [BenchmarkCategory("small"), Benchmark]
//    public async Task<HttpResponseMessage> SmallUtf8()
//    {
//      return await Get(utf8Client, "/littlehydra", "application/ld+json");
//    }

    async Task<HttpResponseMessage> Get(HttpClient httpClient, string uri, string accept)
    {
      var httpResponseMessage = await httpClient.SendAsync(new HttpRequestMessage()
      {
        RequestUri = new Uri($"http://localhost{uri}"),
        Headers = {Accept = {new MediaTypeWithQualityHeaderValue(accept)}}
      }, HttpCompletionOption.ResponseContentRead);
      
      return httpResponseMessage.EnsureSuccessStatusCode();
    }
  }
}