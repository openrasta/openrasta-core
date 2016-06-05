using OpenRasta.Graph;

namespace ResourceModel {
  class has_resource : contexts.resource_model {
    has_resource() {
     given_configuration(new ResourceConfiguration());
    }
  }
}