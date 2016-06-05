using System.Collections.Generic;

namespace OpenRasta.Graph {
  public class ResourceConfiguration {
   IDictionary<string,object> _store = new Dictionary<string,object>();
    
  }

  public class TestingStuff {
    static void Main() {
      new ResourceConfiguration()
        .Resource<Stuff>()
        .Uri(_ => _/"stuff");
    }
  }

  internal class Stuff {
    public int Id { get; set; }
  }
}