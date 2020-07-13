using System.Linq;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Hosting.Katana;
using OpenRasta.Web;
using Owin;

namespace TestingKeepAlive.Web
{
  public class Config
  {
    public void Configuration(IAppBuilder app)
    {
      var openRastaConfiguration = new OpenRasta();

      app.UseOpenRasta(openRastaConfiguration);
    }

    class OpenRasta : IConfigurationSource
    {
      public void Configure()
      {
        ResourceSpace.Has
          .ResourcesOfType<Resource>()
          .AtUri("/")
          .HandledBy<Handler>()
          .TranscodedBy<NewtonsoftJsonCodec>();
      }
    }

    class Resource
    {
      public int[] Data { get; set; }
    }

    class Handler
    {
      public Resource Get() => new Resource
      {
        Data = Enumerable.Range(1, 10000).ToArray()
      };

      public OperationResult Post(Resource resource) => new OperationResult.NoContent();
    }

  }
}