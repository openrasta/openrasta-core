using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
namespace OpenRastaDemo.Benchmark.UriTemplates
{
  [SimpleJob(RuntimeMoniker.Net48)]
  [SimpleJob(RuntimeMoniker.NetCoreApp21)]
  [HtmlExporter,MemoryDiagnoser]
  public class Benchmark
  {
    IMetaModelRepository repository;
    IDependencyResolver resolver;
    InMemoryHost host;
    IUriResolver uriResolver;

    [GlobalSetup]
    public void Setup()
    {
      resolver = new InternalDependencyResolver();
      host = new InMemoryHost(new UriConfigurations(), resolver);
      uriResolver = resolver.Resolve<IUriResolver>();
    }


    [Benchmark]
    public void ParseStringUriTemplates()
    {
      var match = uriResolver.Match(ExpectedUri);
    }

    static readonly Uri ExpectedUri = new Uri("http://localhost/events/ids/1");
    Uri LocalHost = new Uri("http://localhost/");

    [Benchmark]
    public void GenerateStringUriTemplatesFromTypes()
    {
      uriResolver.CreateUriFor(LocalHost, typeof(EventMappedWithStrings), new EventMappedWithStrings(1));
    }
  }

//  public class BenchmarkTests
//  {
//    [Fact]
//    public void can_run_benchmarks()
//    {
//      var benchmark = new Benchmark();
//      benchmark.Setup();
//      benchmark.ParseStringUriTemplates();
//      benchmark.GenerateStringUriTemplatesFromTypes();
//    }
//  }
}