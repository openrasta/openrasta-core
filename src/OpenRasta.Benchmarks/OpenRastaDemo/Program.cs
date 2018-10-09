using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRastaDemo.Shared;

namespace OpenRastaDemo
{
  class Program
  {
    static void Main(string[] args)
    {
      var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "large.json"));

      DemoJsonResponse.LargeJson = JsonConvert.DeserializeObject<IList<RootResponse>>(json);
      DemoHydraResponse.LargeJson = JsonConvert.DeserializeObject<List<HydraRootResponse>>(json);

      var host = new WebHostBuilder()
        .UseKestrel()
        .ConfigureLogging((logging) =>
        {
          logging.AddConsole().SetMinimumLevel(LogLevel.Trace);
          logging.AddDebug().SetMinimumLevel(LogLevel.Trace);
        })
        .UseContentRoot(Directory.GetCurrentDirectory())
        .ConfigureServices(s => s.AddSingleton<IConfigurationSource>(new DemoConfigurationSource("http://localhost:5000/demoreverseproxy")))
        .UseStartup<Startup>()
        .Build();

      host.Run();
    }
  }
}