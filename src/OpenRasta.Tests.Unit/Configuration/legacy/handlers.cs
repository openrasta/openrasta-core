using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.Testing;
using OpenRasta.Tests.Unit.Configuration;
using OpenRasta.TypeSystem;

namespace LegacyManualConfiguration_Specification
{
    public class handlers : configuration_context
    {
        public handlers()
        {
            
            given_resource<Customer>("/customer").HandledBy<CustomerHandler>();
        }
        IType ThenTheUriHasTheHandler<THandler>(string uri)
        {
            var urimatch = DependencyManager.Uris.Match(new Uri(new Uri("http://localhost/", UriKind.Absolute), uri));
            urimatch.ShouldNotBeNull();

            var handlerMatch = DependencyManager.Handlers.GetHandlerTypesFor(urimatch.ResourceKey).FirstOrDefault();
            handlerMatch.ShouldNotBeNull();
            handlerMatch.ShouldBe(TypeSystems.Default.FromClr(typeof(THandler)));
            return handlerMatch;
        }

        [Test]
        public void the_handler_is_registered()
        {

            WhenTheConfigurationIsFinished();

            ThenTheUriHasTheHandler<CustomerHandler>("/customer");
        }
    }
}