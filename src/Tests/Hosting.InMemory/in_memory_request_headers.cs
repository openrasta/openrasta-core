using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Hosting.InMemory
{
  public class in_memory_request_headers
  {
    InMemoryRequest _request;

    public in_memory_request_headers()
    {
      _request = new InMemoryRequest();
    }

    [Fact]
    public void contains_host_header()
    {
      _request.Headers.ContainsKey("Host").ShouldBeTrue();
    }
  }
}
