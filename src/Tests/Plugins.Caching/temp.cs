using System;
using System.Threading.Tasks;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching
{
  public class temp
  {
    [Theory]
    [InlineData("?deepchecks", true)]
    [InlineData("?deepchecks=true", true)]
    [InlineData("?deepchecks=false", false)]
    [InlineData("?deepchecks=FALSE", false)]
    public async Task TestDeepHealthCheckQueryParam(string querystring, bool deepHealthChecksAttempted)
    {
      using (var host = new InMemoryHost(new TestConfig(),startup: new StartupProperties{OpenRasta = { Errors = { HandleAllExceptions = false, HandleCatastrophicExceptions = false }}}))
      {
        var request =
          new InMemoryRequest
          {
            HttpMethod = "GET",
            Uri = new Uri($"http://localhost/healthcheck/{querystring}")
          };
        var response = await host.ProcessRequestAsync(request);
        // response.Should().NotBeNull("whether deep checks are done or not the controller should respond");
        response.StatusCode.ShouldBe(200, "both deep and shallow checks return success");
        // DeepHealthChecks(deepHealthChecksAttempted);
      }
    }
  }

  public class TestConfig : IConfigurationSource
  {
    class Handler
    {
      public string Get(bool enabled = false) => enabled.ToString();
    }
    public void Configure()
    {
      ResourceSpace.Has.ResourcesNamed("Blah")
        .AtUri("/healthcheck/?deepchecks={enabled}")
        .HandledBy(() => new Handler());
    }

  }
  
}