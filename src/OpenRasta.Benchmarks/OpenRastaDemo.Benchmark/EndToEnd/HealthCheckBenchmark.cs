using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Concordia;
using OpenRasta.Hosting.AspNetCore;
using OpenRasta.Hosting.InMemory;
using OpenRasta.IO;

namespace OpenRastaDemo.Benchmark.EndToEnd
{
  [SimpleJob(RuntimeMoniker.Net48)]
  [SimpleJob(RuntimeMoniker.Net60)]
  [HtmlExporter]
  [MemoryDiagnoser]
  // [InProcess()]
  public class HealthCheckBenchmark
  {
    InMemoryHost _memServer;
    TestServer _testServer;
    HttpClient _testServerClient;
    InMemoryHost _memServerNoTrace;

    public HealthCheckBenchmark()
    {
      _memServerNoTrace = new InMemoryHost(new SimpleApi(),startup: new StartupProperties()
      {
        OpenRasta = { Diagnostics = { TracePipelineExecution = false }}
      });

      _testServer = new TestServer(new WebHostBuilder()
        .Configure(builder => builder.UseOpenRasta(new SimpleApi())));

      _testServerClient = _testServer.CreateClient();
    }

    [Benchmark(Baseline = true)]
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