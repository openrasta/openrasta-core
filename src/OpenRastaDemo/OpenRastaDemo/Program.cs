using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenRasta.Configuration;

namespace OpenRastaDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "large.json"));

            DemoJsonResponse.LargeJson = JsonConvert.DeserializeObject<IList<RootResponse>>(json);

            var host = new WebHostBuilder()
                .UseKestrel()
                .ConfigureLogging((logging) =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(new DemoConfigurationSource()))
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}