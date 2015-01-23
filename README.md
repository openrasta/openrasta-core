openrasta-castle-windsor
========================

[![Build status](https://ci.appveyor.com/api/projects/status/68rfy3jpiwd63fdv)](https://ci.appveyor.com/project/holytshirt/openrasta-castle-windsor)

Integrates Castle Windsor with OpenRasta.

Currently built against Castle Windsor 3.3.0 and OpenRasta-core 2.5

To configure OpenRasta that is been used with Asp.Net hosting the configuration for Castle-Windsor is:


```c#
using System.Web;

using Castle.MicroKernel.Registration;
using Castle.Windsor;

using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.DI.Windsor;
using OpenRasta.Hosting.AspNet;

using TestOpenRastaWeb.Handlers;
using TestOpenRastaWeb.Resources;

namespace TestOpenRastaWeb
{
    public class Configuration : IConfigurationSource, IDependencyResolverAccessor
    {
        private WindsorContainer _windsorContainer;

        public void Configure()
        {
            using (OpenRastaConfiguration.Manual)
            {
                ResourceSpace.Has.ResourcesOfType<HelloWorld>().AtUri("/helloworld").HandledBy<HelloWorldHandler>().AsXmlDataContract();
            }
        }

        public IDependencyResolver Resolver
        {
            get
            {
                _windsorContainer = new WindsorContainer();

                //Register stubs to stop Windsor complaining 
                _windsorContainer.Register(
                    Component.For<HttpContext>().UsingFactoryMethod(() => (HttpContext)null),
                    Component.For<AspNetRequest>().UsingFactoryMethod(() => (AspNetRequest)null),
                    Component.For<AspNetResponse>().UsingFactoryMethod(() => (AspNetResponse)null));

                return new WindsorDependencyResolver(_windsorContainer);
            }
        }
    }
}
```
