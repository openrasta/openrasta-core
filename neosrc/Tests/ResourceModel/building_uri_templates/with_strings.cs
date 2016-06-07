using Is;
using OpenRasta.Graph;
using Xunit;

namespace ResourceModel.building_uri_templates {
  public class with_strings {
    [Fact]
    public void can_write() {
      new UriDefinition<TestingStuff>(_ => _/"first"/"second")
        .CreateUri(new TestingStuff()).Is("/first/second/");
    }
  }
}