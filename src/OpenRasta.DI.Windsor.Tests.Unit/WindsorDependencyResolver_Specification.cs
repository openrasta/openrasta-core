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
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using InternalDependencyResolver_Specification;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.DI.Windsor;
using Shouldly;

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
    public class when_registering_for_per_request_lifetime_with_the_castle_resolver : when_registering_for_per_request_lifetime
    {
        public override IDependencyResolver CreateResolver()
        {
            return new WindsorDependencyResolver(new WindsorContainer());
        }

        [Test, Ignore("This test is to surface an issue with the windsor intergration. There is a fix to get around it until we have rewritten the intergration")]
        public override void a_type_registered_as_transient_gets_an_instance_which_is_created_with_another_instance_and_is_registered_as_perwebrequest()
        {
            a_type_registered_as_transient_gets_an_instance_which_is_created_with_another_instance_and_is_registered_as_perwebrequest();
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