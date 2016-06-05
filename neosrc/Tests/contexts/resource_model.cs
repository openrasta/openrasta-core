using OpenRasta.Graph;
namespace contexts{
  public abstract class resource_model {
      private ResourceConfiguration configuration;

      public void given_configuration(ResourceConfiguration configuration) {
      this.configuration = configuration;
    }
  }
}