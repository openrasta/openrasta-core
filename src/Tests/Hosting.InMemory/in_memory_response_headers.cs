using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Hosting.InMemory
{
  public class in_memory_response_headers
  {
    IResponse _response;
    InMemoryHost _host;

    public in_memory_response_headers()
    {
      _host = new InMemoryHost();
    }

    [Fact]
    public async void contains_date_header()
    {
      _response = await _host.ProcessRequestAsync(new InMemoryRequest());

      _response.Headers.ContainsKey("Date").ShouldBeTrue();
    }
  }
}
