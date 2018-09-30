using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Hosting.AspNetCore;

namespace OpenRastaDemo
{
  class Program
  {
    static void Main(string[] args)
    {
      var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "large.json"));

      var host = new WebHostBuilder()
        .UseKestrel()
        .ConfigureLogging((logging) =>
        {
          logging.AddConsole().SetMinimumLevel(LogLevel.Trace);
          logging.AddDebug().SetMinimumLevel(LogLevel.Trace);;
        })
        .Configure(builder => builder.UseOpenRasta(new HydraApi(true,json)))
        .UseContentRoot(Directory.GetCurrentDirectory())
        .Build();

      host.Run();
    }
  }
}