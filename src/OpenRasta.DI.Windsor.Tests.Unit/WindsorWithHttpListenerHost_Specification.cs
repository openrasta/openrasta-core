using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.DI.Windsor;
using OpenRasta.Hosting.HttpListener;
using OpenRasta.Web;

namespace WindsorWithHttpListenerHost_Specification
{
  
  public class MyCustomDependencyNoFactory
  {
    
  }
  public class MyCustomDependency0
  {
    
  }
  public class MyCustomDependency1
  {
    readonly IDependencyResolver _resolver;


    public MyCustomDependency1(IDependencyResolver resolver)
    {
      _resolver = resolver;
    }
  }

  public class MyCustomDependency2
  {
    readonly IDependencyResolver _resolver;
    readonly IRequest _request;

    public MyCustomDependency2(IDependencyResolver resolver, IRequest request)
    {
      _resolver = resolver;
      _request = request;
    }
  }

  public class MyCustomDependency3
  {
    readonly IDependencyResolver _resolver;
    readonly IRequest _request;
    readonly IResponse _response;


    public MyCustomDependency3(IDependencyResolver resolver, IRequest request, IResponse response)
    {
      _resolver = resolver;
      _request = request;
      _response = response;
    }
  }

  public class MyCustomDependency4
  {
    readonly IDependencyResolver _resolver;
    readonly IRequest _request;
    readonly IResponse _response;
    readonly ICommunicationContext _context;


    public MyCustomDependency4(IDependencyResolver resolver, IRequest request, IResponse response, ICommunicationContext context)
    {
      _resolver = resolver;
      _request = request;
      _response = response;
      _context = context;
    }
  }
  class TestConfigurationSource : IConfigurationSource
  {
    public void Configure()
    {
      ResourceSpace.Has.ResourcesOfType<string>()
        .AtUri("/").Named("Root")
        .And.AtUri("/with-header").Named("WithHeader")
        .And.AtUri("/dependency").Named("Dependency")
        .HandledBy<TestHandler>()
        .And.HandledBy<HeaderSettingHandler>()
        .And.HandledBy<TestDependencyHandler>()
        .TranscodedBy<TextPlainCodec>();

      
      ResourceSpace.Uses
        .Dependency(ctx => ctx.Transient(()=>new MyCustomDependency0()))
        .Dependency(ctx=>ctx.Singleton<MyCustomDependencyNoFactory>());
      ResourceSpace.Uses.Dependency(ctx => 
        ctx.Transient((IDependencyResolver resolver) => 
          new MyCustomDependency1(resolver)));
      ResourceSpace.Uses.Dependency(ctx => 
        ctx.Transient((IDependencyResolver resolver, IRequest request) => 
          new MyCustomDependency2(resolver, request)));
      ResourceSpace.Uses.Dependency(ctx => 
        ctx.Transient((IDependencyResolver resolver, IRequest request, IResponse response) => 
        new MyCustomDependency3(resolver, request, response)));
      ResourceSpace.Uses.Dependency(ctx => 
        ctx.Transient((IDependencyResolver resolver, IRequest request, IResponse response, ICommunicationContext context) => 
          new MyCustomDependency4(resolver, request, response, context)));
    }
  }

  public class TestHandler
  {
    [HttpOperation(ForUriName = "Root")]
    public string Get()
    {
      return "Test Root Response";
    }
  }

  public class TestDependencyHandler
  {
    readonly object[] _dependencies;

    public TestDependencyHandler(MyCustomDependencyNoFactory depNoFactory,
      MyCustomDependency0 dependency0,
      MyCustomDependency1 dependency1,
      MyCustomDependency2 dependency2,
      MyCustomDependency3 dependency3,
      MyCustomDependency4 dependency4)
    {
      _dependencies = new object[] {depNoFactory,dependency0, dependency1, dependency2, dependency3, dependency4};
    }

    [HttpOperation(ForUriName = "Dependency")]
    public string Get()
    {
      if (_dependencies.Any(d=>d == null))
        throw new InvalidOperationException("Dependencies are borked");
      return "Test Dependency Response";
    }
  }

  public class HeaderSettingHandler
  {
    readonly IResponse _response;

    public HeaderSettingHandler(IResponse response)
    {
      _response = response;
    }

    [HttpOperation(ForUriName = "WithHeader")]
    public string Get()
    {
      _response.Headers["FOO"] = "BAR";
      return "Test Header Response";
    }
  }

  public class when_creating_a_new_HttpListenerHost_with_WindsorResolver : IDisposable
  {
    static Random randomPort = new Random();
    HttpListenerHost _host;
    string Prefix;
    private IWindsorContainer _container;

    public when_creating_a_new_HttpListenerHost_with_WindsorResolver()
    {
      // init container
      _container = new WindsorContainer();
      var port = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 18981 : randomPort.Next(1024, 2048);

      Prefix = $"http://localhost:{port}/";

      _container.Register(
        Component.For<IConfigurationSource>().ImplementedBy<TestConfigurationSource>().LifestyleSingleton());


      _host = new HttpListenerHost(new TestConfigurationSource(), new WindsorDependencyResolver(_container));
      _host.Initialize(new[] {Prefix}, "/");
      _host.StartListening();
    }


    public void Dispose()
    {
      _host.StopListening();
      _host.Close();
    }

    [Test]
    public void the_resolver_is_a_windsor_dependency_resolver()
    {
      Assert.That(_host.Resolver, Is.Not.Null);
      Assert.That(_host.Resolver, Is.InstanceOf<WindsorDependencyResolver>());
    }

    [Test]
    public void the_root_uri_serves_the_test_string()
    {
      var response = new WebClient().DownloadString(Prefix);
      Assert.That(response, Is.EqualTo("Test Root Response"));
    }

    [Test]
    public void dependencies_are_resolved()
    {
      var response = new WebClient().DownloadString(Prefix + "dependency");
      Assert.That(response, Is.EqualTo("Test Dependency Response"));
    }

    [Test]
    public void the_with_header_uri_serves_the_response_header()
    {
      var webClient = new WebClient();
      var response = webClient.DownloadString(Prefix + "with-header");
      Assert.That(response, Is.EqualTo("Test Header Response"));

      var fooHeader = webClient.ResponseHeaders["FOO"];
      Assert.That(fooHeader, Is.EqualTo("BAR"));
    }
  }
}