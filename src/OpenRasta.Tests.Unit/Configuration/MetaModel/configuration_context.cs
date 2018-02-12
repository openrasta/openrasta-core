using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Implementation;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.DI;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.TypeSystem;
using OpenRasta.TypeSystem.ReflectionBased;

namespace Configuration_Specification
{
  public abstract class configuration_context : context
  {
    protected static IHas ResourceSpaceHas;
    protected static IUses ResourceSpaceUses;
    protected IMetaModelRepository MetaModel;
    protected IDependencyResolver Resolver;

    protected ResourceModel FirstRegistration
    {
      get { return MetaModel.ResourceRegistrations.First(); }
    }

    protected override void SetUp()
    {
      base.SetUp();
      Resolver = new InternalDependencyResolver();
      Resolver.AddDependency<ITypeSystem, ReflectionBasedTypeSystem>();
      MetaModel = new MetaModelRepository(Resolver.Resolve<IEnumerable<IMetaModelHandler>>);
      ResourceSpaceHas = new FluentTarget(Resolver, MetaModel);
      ResourceSpaceUses = new FluentTarget(Resolver, MetaModel);

      DependencyManager.SetResolver(Resolver);
    }

    protected override void TearDown()
    {
      base.TearDown();
      DependencyManager.UnsetResolver();
      if (DependencyManager.Current != null) throw new InvalidOperationException("FUCK");
    }
  }
}