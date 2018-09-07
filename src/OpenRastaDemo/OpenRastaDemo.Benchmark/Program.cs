using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
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
                .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(new DemoConfigurationSource()))
                .UseStartup<Startup>()
            );
            
            this.client = this.server.CreateClient();
            this.client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        [Benchmark]
        public async Task<HttpResponseMessage> GetMeSomeJson()
        {
            return await this.client.GetAsync("/");
        }

        [Benchmark]
        public async Task<HttpResponseMessage> GetMeSomeLittleJson()
        {
            return await this.client.GetAsync("/littlejson");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
#if DEBUG
            var benchMark = new JsonBenchmark();
            benchMark.Setup();
            var response = await benchMark.GetMeSomeLittleJson();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            
#else
            var summary = BenchmarkRunner.Run<JsonBenchmark>();
#endif
        }
    }
}