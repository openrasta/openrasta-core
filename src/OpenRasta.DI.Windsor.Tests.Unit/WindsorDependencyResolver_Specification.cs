#region License

/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */

#endregion

using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using InternalDependencyResolver_Specification;
using NUnit.Framework;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.DI.Windsor;
using OpenRasta.Hosting;
using OpenRasta.Pipeline;
using OpenRasta.Tests.Unit.DI;
using Shouldly;
using IDependencyResolver = OpenRasta.DI.IDependencyResolver;

namespace WindsorDependencyResolver_Specification
{
  [TestFixture]
  public class when_resolving_instances_the_castle_resolver : when_resolving_instances
  {
    public override IDependencyResolver CreateResolver()
    {
      return new WindsorDependencyResolver(new WindsorContainer());
    }
  }


  [TestFixture]
  public class when_registering_dependencies_with_the_castle_resolver : when_registering_dependencies
  {
    readonly WindsorContainer _container = new WindsorContainer();


    public override IDependencyResolver CreateResolver()
    {
      return new WindsorDependencyResolver(_container);
    }


    [Test]
    public void then_it_should_be_regarded_as_dependency_regardless_of_its_state()
    {
      var serviceToBeResolved = typeof(DependencyOnTypeWithGernericParams);
      const String dynamicDependency = "TestDependency";

      _container.Register(Component.For(serviceToBeResolved)
        .DynamicParameters((k, d) => d["dependency"] = dynamicDependency));

      Resolver.HasDependency(serviceToBeResolved).ShouldBe(true);
      Resolver.Resolve<DependencyOnTypeWithGernericParams>().Dependency.ShouldBe(dynamicDependency);
    }
  }

  [TestFixture]
  public class
    registration_depending_on_func_in_other_scope_with_the_castle_resolver :
      registration_depending_on_func_in_other_scope
  {
    public override IDependencyResolver CreateResolver()
    {
      return new WindsorDependencyResolver(new WindsorContainer());
    }
  }

  [TestFixture]
  public class
    registration_depending_on_func_of_unregistered_with_the_castle_resolver :
      registration_depending_on_func_of_unregistered
  {
    public override IDependencyResolver CreateResolver()
    {
      return new WindsorDependencyResolver(new WindsorContainer());
    }

    [Test]
    public override void cannot_resolve_func_before_type_is_registered()
    {
      Resolver.AddDependency<DependsOnFuncOfSimple>();

      var dependent = Resolver.Resolve<DependsOnFuncOfSimple>();
      Should.Throw<ComponentNotFoundException>(() =>
      {
        var simple = dependent.Simple();
        simple.ShouldNotBeNull();
      });
    }
  }


  [TestFixture]
  public class
    registration_depending_on_func_of_unregistered_after_registration_with_the_castle_resolver :
      registration_depending_on_func_of_unregistered_after_registration
  {
    public override IDependencyResolver CreateResolver()
    {
      return new WindsorDependencyResolver(new WindsorContainer());
    }
  }

  [TestFixture]
  public class
    registration_depending_on_enum_of_unregistered_after_registration_with_the_castle_resolver :
      registration_depending_on_enum_of_unregistered_after_registration
  {
    public override IDependencyResolver CreateResolver()
    {
      return new WindsorDependencyResolver(new WindsorContainer());
    }
  }

  [TestFixture]
  public class
    registration_profiles_with_the_castle_resolver :
      registration_profiles
  {
    WindsorContainer _container;

    public registration_profiles_with_the_castle_resolver()
    {
      _container = new WindsorContainer();
    }

    public override IDependencyResolver CreateResolver()
    {
      return new WindsorDependencyResolver(_container);
    }

    [Test]
    public void resolve_factory_enums()
    {
      var modelAware = (IModelDrivenDependencyRegistration) Resolver;

      var typeRegistration = new TypeRegistrationContext();
      typeRegistration.Singleton(() => new Simple());
      modelAware.Register(typeRegistration.Model);
      
      typeRegistration = new TypeRegistrationContext();
      typeRegistration.Singleton((IEnumerable<Simple> simples) => new Complex(simples));

      var simples2 = Should.NotThrow(() => Resolver.Resolve<IEnumerable<Simple>>());
      simples2.ShouldHaveSingleItem().ShouldNotBeNull();

      var complex = Should.NotThrow(() => Resolver.Resolve<Complex>());
      complex.Simple.ShouldHaveSingleItem().ShouldNotBeNull();
    }

    public class Complex
    {
      public IEnumerable<Simple> Simple { get; }

      public Complex(IEnumerable<Simple> simple)
      {
        Simple = simple;
      }
    }
  }


  [TestFixture]
  public class when_creating_resolver_and_dependency_resolver_all_ready_registered
  {
    [Test]
    public void then_it_should_not_error()
    {
      var windsorContainer = new WindsorContainer();
      windsorContainer.Register(Component.For<IDependencyResolver>().ImplementedBy<FakeResolver>());
      var windsorDependencyResolver = new WindsorDependencyResolver(windsorContainer);

      windsorContainer.Resolve<IDependencyResolver>().ShouldBeAssignableTo<FakeResolver>();
      windsorContainer.Resolve<IModelDrivenDependencyRegistration>().ShouldBeAssignableTo<WindsorDependencyResolver>();
    }
  }

  public class FakeResolver : IDependencyResolver
  {
    public bool HasDependency(Type serviceType)
    {
      throw new NotImplementedException();
    }

    public bool HasDependencyImplementation(Type serviceType, Type concreteType)
    {
      throw new NotImplementedException();
    }

    public void AddDependency(Type concreteType, DependencyLifetime lifetime)
    {
      throw new NotImplementedException();
    }

    public void AddDependency(Type serviceType, Type concreteType, DependencyLifetime dependencyLifetime)
    {
      throw new NotImplementedException();
    }

    public void AddDependencyInstance(Type registeredType, object value, DependencyLifetime dependencyLifetime)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<TService> ResolveAll<TService>()
    {
      throw new NotImplementedException();
    }

    public object Resolve(Type type)
    {
      throw new NotImplementedException();
    }

    public void HandleIncomingRequestProcessed()
    {
      throw new NotImplementedException();
    }
  }
}

#region Full license

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#endregion