using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;
#if NET5_0_OR_GREATER
using Microsoft.Extensions.Hosting;
#endif
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
#endif
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.Owin;

namespace OpenRasta.Hosting.AspNetCore
{
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseOpenRasta(
      this IApplicationBuilder app,
      IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null,
      StartupProperties properties = null)
    {

      return app
        .Use(async (context, next) =>
        {
          var auth = context.Features.Get<IHttpAuthenticationFeature>();
          if (auth == null)
          {
            auth = new HttpAuthenticationFeature();
            context.Features.Set(auth);
          }

          await next();
        })
        .UseOwin(builder =>
        {
#if NET5_0_OR_GREATER
          var onAppDisposing = app.ApplicationServices.GetService<IHostApplicationLifetime>().ApplicationStopping;
#endif
#if NETSTANDARD2_0
          var onAppDisposing = app.ApplicationServices.GetService<IApplicationLifetime>().ApplicationStopping;
#endif

          builder.UseOpenRasta(
            configurationSource,
            dependencyResolver,
            onAppDisposing, properties);
        });
    }
  }
}