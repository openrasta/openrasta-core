using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using ICodec = OpenRasta.Codecs.ICodec;

namespace OpenRasta.Plugins.Hydra
{
  public static class Configuration
  {
    public static IUses Hydra(this IUses uses)
    {
      var has = (IHas) uses;

      has.ResourcesOfType<IJsonLdDocument>()
        .WithoutUri
        .TranscodedBy<JsonLdCodec>()
        .ForMediaType("application/ld+json");
      
      has
        .ResourcesOfType<EntryPoint>()
        .AtUri("/")
        .HandledBy<EntryPointHandler>();
      has
        .ResourcesOfType<RootContext>()
        .AtUri("/.hydra/context.jsonld")
        .HandledBy<RootContextHandler>();


      return uses;
    }
    
  }
}