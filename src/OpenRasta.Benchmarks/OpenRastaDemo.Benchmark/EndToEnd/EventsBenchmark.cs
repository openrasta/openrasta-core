using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Concordia;
using OpenRasta.Hosting.AspNetCore;
using OpenRasta.Hosting.InMemory;
using OpenRasta.IO;

namespace OpenRastaDemo.Benchmark.EndToEnd
{
  // [SimpleJob(RuntimeMoniker.Net48)]
  // [SimpleJob(RuntimeMoniker.NetCoreApp21)]
  // [SimpleJob(RuntimeMoniker.NetCoreApp31)]
  // [HtmlExporter]
  [MemoryDiagnoser]
  [InProcess()]
  public class EventsBenchmark
  {
    InMemoryHost _memServer;
    TestServer _testServer;
    HttpClient _testServerClient;
    InMemoryHost _memServerNoTrace;

    public EventsBenchmark()
    {
      _memServerNoTrace = new InMemoryHost(new SimpleApi(),startup: new StartupProperties()
      {
        OpenRasta = { Diagnostics = { TracePipelineExecution = false }}
      });

      _testServer = new TestServer(new WebHostBuilder()
        .Configure(builder => builder.UseOpenRasta(new SimpleApi())));

      _testServerClient = _testServer.CreateClient();
    }

    object SetupKestrel(SimpleApi simpleApi)
    {
      throw new NotImplementedException();
    }

    [BenchmarkCategory("GET /events/"), Benchmark(Baseline = true)]
    public async Task<byte[]> GetAllEventsMemory()
    {
      var response = await _memServerNoTrace.ProcessRequestAsync(new InMemoryRequest()
      {
        HttpMethod = "GET",
        Uri = new Uri("http://localhost/events/")
      });
      var body = await response.Entity.Stream.ReadToEndAsync();
      VerifyResult(response.StatusCode, body);
      return body;
    }
    
    static void VerifyResult(int responseStatusCode, byte[] body)
    {
      
      if (responseStatusCode != 200 || body.Length <= 0)
        throw new InvalidOperationException($"Body length: {body.Length}, body: {Encoding.UTF8.GetString(body)}");
    }

    [BenchmarkCategory("GET /events/"),Benchmark()]
    public async Task<byte[]> GetAllEventsTestServer()
    {
      var response = await _testServerClient.GetAsync("http://localhost/events/");
      var body = await response.Content.ReadAsByteArrayAsync();
      VerifyResult((int) response.StatusCode, body);
      return body;
    }
  }
}