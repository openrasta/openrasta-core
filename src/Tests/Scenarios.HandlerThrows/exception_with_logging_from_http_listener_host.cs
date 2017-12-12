using System;
using System.Linq;
using System.Net;
using System.Reflection;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Hosting.HttpListener;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerThrows
{
  public class exception_with_logging_from_http_listener_host : IDisposable
  {
    readonly FakeLogger _fakeLogger;
    readonly HttpListenerHost _httpListenerHost;
    readonly HttpWebResponse _response;

    public exception_with_logging_from_http_listener_host()
    {
      _fakeLogger = new FakeLogger();

      var appPathVDir = $"Temporary_Listen_Addresses/{Guid.NewGuid()}/";
      
      var port = new Random().Next(1024,2048);

      _httpListenerHost = new HttpListenerHost(new Configuration(_fakeLogger));
      _httpListenerHost.Initialize(new [] { $"http://+:{port}/{appPathVDir}" }, appPathVDir, null);
      _httpListenerHost.StartListening();

      using (var webClient = new WebClient())
      {
        try
        {
          webClient.DownloadString($"http://localhost:{port}/{appPathVDir}");
        }
        catch (WebException e)
        {
          _response = (HttpWebResponse) e.Response;
        }
      }
    }

    [Fact]
    public void gives_500_status() => _response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    [Fact]
    public void logs_an_exception() => _fakeLogger.Exceptions.ShouldHaveSingleItem();
    [Fact]
    public void logs_correct_exception() => _fakeLogger.Exceptions.Single().ShouldBeOfType<TargetInvocationException>();
    [Fact]
    public void logs_correct_inner_exception() => _fakeLogger.Exceptions.Single().InnerException?.Message.ShouldBe("This is an exception");

    class Configuration : IConfigurationSource
    {
      readonly FakeLogger _fakeLogger;

      public Configuration(FakeLogger fakeLogger)
      {
        _fakeLogger = fakeLogger;
      }

      public void Configure()
      {
        using (OpenRastaConfiguration.Manual)
        {
          ResourceSpace.Uses.Resolver.AddDependencyInstance(
            typeof(ILogger),
            _fakeLogger,
            DependencyLifetime.Singleton);

          ResourceSpace.Has.ResourcesNamed("root")
            .AtUri("/")
            .HandledBy<ThrowingHandler>().TranscodedBy<TextPlainCodec>();
        }
      }
    }

    public void Dispose()
    {
      ((IDisposable) _httpListenerHost).Dispose();
      _response.Dispose();
    }
  }
}