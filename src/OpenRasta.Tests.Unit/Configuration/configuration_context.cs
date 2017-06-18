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
    private Action _configuration;
    private bool _resolverSet;

    protected override void SetUp()
    {
      base.SetUp();
    }

    protected override void TearDown()
    {
      base.TearDown();
      _host?.Close();
      if (_resolverSet)
        DependencyManager.UnsetResolver();
      _resolverSet = false;
    }

    public virtual void WhenTheConfigurationIsFinished()
    {
      _host = new InMemoryHost(_configuration);
      DependencyManager.SetResolver(_host.Resolver);
      _resolverSet = true;
    }

    public void GivenAResourceRegistrationFor<TResource>(
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