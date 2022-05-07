using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using OpenRasta.Web;

namespace OpenRastaDemo.Benchmark.Uris
{
  [SimpleJob(RuntimeMoniker.Net48)]
  [SimpleJob(RuntimeMoniker.Net60)]
  [HtmlExporter,MemoryDiagnoser]
  public class Matching
  {
    TemplatedUriResolver _templatedUriResolver;

    public Matching()
    {
      _templatedUriResolver = new TemplatedUriResolver()
      {
        new UriRegistration("/events/", new object()),
        new UriRegistration("/events/{id}", new object()),
        new UriRegistration("/events/?by={by}", new object())
      };
    }

    [Benchmark]
    public void Literal()
    {
      _templatedUriResolver.Match(new Uri("https://localhost/events/"));
    }

    [Benchmark]
    public void Segment()
    {
      _templatedUriResolver.Match(new Uri("https://localhost/events/1"));
    }

    [Benchmark]
    public void QueryString()
    {
      _templatedUriResolver.Match(new Uri("https://localhost/events/?by=gracekelly"));
    }
  }
}