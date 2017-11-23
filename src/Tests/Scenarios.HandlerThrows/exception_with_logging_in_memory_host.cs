using System.Linq;
using System.Reflection;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Tests.Scenarios.HandlerSelection;
using Xunit;

namespace Tests.Scenarios.HandlerThrows
{
  public class exception_with_logging_in_memory_host
  {
    readonly FakeLogger _fakeLogger;
    readonly IResponse _response;

    public exception_with_logging_in_memory_host()
    {
      _fakeLogger = new FakeLogger();

      var server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Resolver.AddDependencyInstance(
          typeof(ILogger),
          _fakeLogger,
          DependencyLifetime.Singleton);

        ResourceSpace.Has.ResourcesNamed("root")
          .AtUri("/")
          .HandledBy<ThrowingHandler>().TranscodedBy<TextPlainCodec>();
      });

      _response = server.Get(server.ApplicationVirtualPath).Result;
    }

    [Fact]
    public void gives_500_status() => _response.StatusCode.ShouldBe(500);
    [Fact]
    public void logs_an_exception() => _fakeLogger.Exceptions.ShouldHaveSingleItem();
    [Fact]
    public void logs_correct_exception() => _fakeLogger.Exceptions.Single().ShouldBeOfType<TargetInvocationException>();
    [Fact]
    public void logs_correct_inner_exception() => _fakeLogger.Exceptions.Single().InnerException?.Message.ShouldBe("This is an exception");
  }
}
