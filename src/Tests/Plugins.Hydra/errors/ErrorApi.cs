using System;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Diagnostics;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Hydra.errors
{
  public class ErrorApi : IConfigurationSource

  {
    readonly Func<OperationResult> get;

    public ErrorApi(Func<OperationResult> get)
    {
      this.get = get;
    }

    public void Configure()
    {
      ResourceSpace.Has.ResourcesNamed("errors")
        .Vocabulary("https://localhost/tests/")
        .AtUri("/error")
        .HandledBy(() => this);

      ResourceSpace.Uses.Hydra();
    }

    public OperationResult Get() => get();
  }

  public class exceptions
  {
    [Fact]
    public async Task without_help_url()
    {
      var result = Attribute.GetCustomAttribute(GetType().GetMethod(nameof(without_help_url)), typeof(FactAttribute));
      var result2 = Attribute.GetCustomAttribute(GetType().GetMethod(nameof(without_help_url)), typeof(FactAttribute));
      ReferenceEquals(result, result2).ShouldBeFalse();
    }

    async Task<IResponse> Get(Func<OperationResult> getter)
    {
      var server = new InMemoryHost(new ErrorApi(getter));
      return await server.ProcessRequestAsync(new InMemoryRequest() {HttpMethod = "GET"});
    }
  }
}