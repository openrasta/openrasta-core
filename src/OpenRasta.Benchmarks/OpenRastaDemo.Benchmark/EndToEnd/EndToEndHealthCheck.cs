using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Concordia;
using OpenRasta.DI;
using OpenRasta.DI.Windsor;
using OpenRasta.Hosting.AspNetCore;
using OpenRasta.IO;
using BenchmarkDotNet.Jobs;

namespace OpenRastaDemo.Benchmark.EndToEnd
{
  public class HealthCheckApi : IConfigurationSource, IDependencyResolverAccessor
  {
    class Resource
    {
      public string Status { get; set; }
    }

    class Handler
    {
      public Resource Get() => new Resource() {Status = "pass"};
    }

    public void Configure()
    {
      ResourceSpace.Has.ResourcesOfType<Resource>()
        .AtUri("/.well-known/health")
        .HandledBy<Handler>()
        .AsJsonNewtonsoft();
    }

    public IDependencyResolver Resolver { get; } = new WindsorDependencyResolver();
  }

  [SimpleJob(RuntimeMoniker.Net48)]
  [SimpleJob(RuntimeMoniker.NetCoreApp21)]
  [HtmlExporter,MemoryDiagnoser]
  public class SimpleHealthCheckBenchmark
  {
    InMemoryHost _memServer;
    TestServer _testServer;
    HttpClient _testServerClient;
    InMemoryHost _memServerWithTrace;
    InMemoryHost _memServerNoTrace;

    public SimpleHealthCheckBenchmark()
    {
      _memServerWithTrace = new InMemoryHost(new HealthCheckApi(),startup:new StartupProperties()
      {
        OpenRasta = { Diagnostics = { TracePipelineExecution = true }}
      });
      _memServerNoTrace = new InMemoryHost(new HealthCheckApi(),startup: new StartupProperties()
      {
        OpenRasta = { Diagnostics = { TracePipelineExecution = false }}
      });

      _testServer = new TestServer(new WebHostBuilder()
        .Configure(builder => builder.UseOpenRasta(new HealthCheckApi())));

      _testServerClient = _testServer.CreateClient();
    }

    [Benchmark]
    public async Task<byte[]> MemoryHostNoTrace()
    {
      var response = await _memServerNoTrace.ProcessRequestAsync(new InMemoryRequest()
      {
        HttpMethod = "GET",
        Uri = new Uri("http://localhost/.well-known/health")
      });
      var body = await response.Entity.Stream.ReadToEndAsync();
      VerifyResult(body);
      return body;
    }

    // [Benchmark()]
    // public async Task<byte[]> MemoryHostTrace()
    // {
    //   var response = await _memServerWithTrace.ProcessRequestAsync(new InMemoryRequest()
    //   {
    //     HttpMethod = "GET",
    //     Uri = new Uri("http://localhost/.well-known/health")
    //   });
    //   var body = await response.Entity.Stream.ReadToEndAsync();
    //   VerifyResult(body);
    //   return body;
    // }
    static void VerifyResult(byte[] body)
    {
      if (body.Length != 17)
        throw new InvalidOperationException($"Body length: {body.Length}, body: {Encoding.UTF8.GetString(body)}");
    }

    [Benchmark()]
    public async Task<byte[]> TestServer()
    {
      var response = await _testServerClient.GetAsync("http://localhost/.well-known/health");
      var body = await response.Content.ReadAsByteArrayAsync();
      VerifyResult(body);
      return body;
    }
  }
}