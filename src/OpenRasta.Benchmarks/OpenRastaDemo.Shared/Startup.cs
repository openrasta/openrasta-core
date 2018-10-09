using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenRasta.Configuration;
using OpenRasta.Hosting.AspNetCore;

namespace OpenRastaDemo.Shared
{
  public class Startup
  {
    private readonly IConfigurationSource configurationSource;

    public Startup(IConfigurationSource configurationSource)
    {
      this.configurationSource = configurationSource;
    }

    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app)
    {
      app.UseOpenRasta(this.configurationSource);
    }
  }
}