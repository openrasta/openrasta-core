using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using OpenRasta.Collections;

namespace OpenRastaDemo.Benchmark
{
  class Program
  {
    static async Task Main(string[] args)
    {
      BenchmarkRunner.Run<HydraBenchmark>();
//#if DEBUG
//      var benchMark = new JsonBenchmark();
//      benchMark.Setup();
//      var response = await benchMark.GetMeSomeLittleJson();
//      var content = await response.Content.ReadAsStringAsync();
//      Console.WriteLine(content);
//
//#else
////var summary = BenchmarkRunner.Run<JsonBenchmark>();
//      new BenchmarkSwitcher(typeof(Program).Assembly).RunAll();
//#endif
    }
  }
}