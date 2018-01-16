using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.Katana;

namespace OpenRasta.Hosting.AspNetCore
{
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseOpenRasta(
      this IApplicationBuilder app,
      IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null)
    {
      return app.UseOwin(builder =>
        builder.UseOpenRasta(
          configurationSource,
          dependencyResolver,
          app.ApplicationServices.GetService<IApplicationLifetime>().ApplicationStopping));
    }
  }
}