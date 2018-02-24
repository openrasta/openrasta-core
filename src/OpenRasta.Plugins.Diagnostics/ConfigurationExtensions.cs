using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Diagnostics.Trace;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Diagnostics
{
  public static class ConfigurationExtensions
  {
    public static T Diagnostics<T>(this T anchor, DiagnosticsOptions options = null) where T : IUses
    {
      var has = (IHas) anchor;
      var target = (IFluentTarget) has;
      options = options ?? new DiagnosticsOptions();

      if (options.TraceMethod)
      {
        has.ResourcesOfType<IRequest>()
          .WithoutUri
          .TranscodedBy<RequestMessageCodec>()
          .ForMediaType("message/http");

        foreach (var registration in target.Repository.ResourceRegistrations)
          registration.Handlers.Add(new HandlerModel(TypeSystems.Default.FromClr<TraceHandler>()));
      }

      return anchor;
    }
  }
}