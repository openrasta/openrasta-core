using System;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Hosting;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Tests.Unit.Infrastructure;
using Shouldly;

namespace OpenRasta.Tests.Unit.Configuration
{
  public class configuration_context : context
  {
    InMemoryHost _host;
    Action _configuration;
    bool _resolverSet;
    protected IDependencyResolver Resolver => _host.Resolver;
    
    protected override void TearDown()
    {
      base.TearDown();
      _host?.Close();
      if (_resolverSet)
        DependencyManager.UnsetResolver();
      _resolverSet = false;
    }

    protected void WhenTheConfigurationIsFinished()
    {
      _host = new InMemoryHost(_configuration);
      DependencyManager.SetResolver(_host.Resolver);
      _resolverSet = true;
    }

    protected void GivenAResourceRegistrationFor<TResource>(
      string uri,
      Action<IUriDefinition<TResource>> config = null)
    {
      config = config ?? (u => { });
      _configuration = () => { config(ResourceSpace.Has.ResourcesOfType<TResource>().AtUri(uri)); };
    }

    protected class Customer
    {
    }

    protected class CustomerHandler
    {
    }
  }
}