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
using System.Globalization;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Testing;
using OpenRasta.Tests.Unit.Configuration;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace LegacyManualConfiguration_Specification
{
    public class when_adding_uris_to_a_resource : configuration_context
    {
        void ThenTheUriHasTheResource<TResource>(string uri, CultureInfo language, string name)
        {
            var match = DependencyManager.Uris.Match(new Uri(new Uri("http://localhost/", UriKind.Absolute), uri));
            match.ShouldNotBeNull();
            match.UriCulture.ShouldBe(language);
            match.ResourceKey.ShouldBe(TypeSystems.Default.FromClr(typeof(TResource)));
            match.UriName.ShouldBe(name);
        }

        [Test]
        public void language_and_names_are_properly_registered()
        {
            given_resource<Customer>("/customer")
                .InLanguage("fr").Named("French");

            WhenTheConfigurationIsFinished();

            ThenTheUriHasTheResource<Customer>("/customer", CultureInfo.GetCultureInfo("fr"), "French");
        }

        [Test]
        public void registering_two_urls_works()
        {
            given_resource<Customer>("/customer/{id}")
                .InLanguage("en-CA")
                .AndAt("/privileged/customer/{id}").Named("Privileged");

            WhenTheConfigurationIsFinished();

            ThenTheUriHasTheResource<Customer>("/customer/{id}", CultureInfo.GetCultureInfo("en-CA"), null);
            ThenTheUriHasTheResource<Customer>("/privileged/customer/{id}", null, "Privileged");
        }

        [Test]
        public void the_base_uri_is_registered_for_that_resource()
        {
            given_resource<Customer>("/customer");

            WhenTheConfigurationIsFinished();

            ThenTheUriHasTheResource<Customer>("/customer", null, null);
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