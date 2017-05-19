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
using System.Linq;
using System.Text;
using OpenRasta.Binding;
using OpenRasta.Data;
using OpenRasta.Testing;
using NUnit.Framework;
using OpenRasta.Testing.Contexts;
using OpenRasta.Web;
using Moq;
using System.IO;
using OpenRasta.Codecs;
using OpenRasta.Pipeline;
using OpenRasta.IO;
using Shouldly;

namespace ApplicationXWwwUrlformEncodedCodec_Specification
{
    public class when_the_requested_type_is_a_dictionary : app_www_context
    {
        [Test]
        public void the_values_are_returned()
        {
            given_context();
            given_request_stream("Customer.Something=John&Customer.SomethingElse=Doe");

            when_decoding<Dictionary<string, string[]>>();

            ThenTheResult
                .LegacyShouldContain("Customer.Something", new[] { "John" })
                .LegacyShouldContain("Customer.SomethingElse", new[] { "Doe" });

        }
        private Dictionary<string, string[]> ThenTheResult { get { return base.then_decoding_result<Dictionary<string, string[]>>(); } }
    }
    public class when_parsing_for_simple_types : app_www_context
    {
        [Test]
        public void url_encoding_is_resolved()
        {
            given_context();
            given_request_stream("thecustomer=John%20Doe");

            when_decoding<string>("thecustomer");

            then_decoding_result<string>()
                .LegacyShouldBe("John Doe");
        }
        [Test]
        public void strings_are_assigned()
        {

            given_context();
            given_request_stream("thecustomer=John&thecustomer=Jack");

            when_decoding<string[]>("thecustomer");

          then_decoding_result<string[]>().ShouldBe((IEnumerable<string>) new[] { "John", "Jack" });
        }
        [Test]
        public void string_arrays_are_assigned()
        {
            given_context();
            given_request_stream("thecustomer=John");

            when_decoding<string>("thecustomer");

            then_decoding_result<string>()
                .LegacyShouldBe("John");
        }

        [Test]
        public void guids_are_assigned()
        {
            given_context();
            given_request_stream("myguid=044A624B-466A-4383-89FA-A02B629C78B9");

            when_decoding<Guid>("myguid");

            then_decoding_result<Guid>()
                .LegacyShouldBe(new Guid("044A624B-466A-4383-89FA-A02B629C78B9"));
        }

        [Test]
        public void invalid_guids_are_assigned()
        {
            given_context();
            given_request_stream("myguid=xxx");

            when_decoding<Guid>("myguid");

            then_decoding_result_is_missing();
        }

        [Test]
        public void invalid_nullable_guids_are_assigned()
        {
            given_context();
            given_request_stream("myguid=xxx");

            when_decoding<Guid?>("myguid");

            then_decoding_result_is_missing();
        }

        [Test]
        public void integers_are_assigned()
        {

            given_context();
            given_request_stream("thecustomerid=3");

            when_decoding<int>("thecustomerid");

            then_decoding_result<int>()
                .LegacyShouldBe(3);
        }
    }
    public class when_parsing_for_complex_types : app_www_context
    {
        [Test]
        public void the_complex_type_is_built_when_referenced_by_name()
        {
            given_context();
            given_request_stream("thecustomer.FirstName=John");

            when_decoding<Customer>("thecustomer");

            then_decoding_result<Customer>().FirstName
                .LegacyShouldBe("John");

        }
        [Test]
        public void the_complex_type_is_built_from_a_simple_value()
        {
            given_context();
            given_request_stream("Customer=John");

            when_decoding<Customer>();

            then_decoding_result<Customer>().FirstName
                .LegacyShouldBe("John");
        }
        [Test]
        public void multiple_values_for_a_non_array_type_are_ignored()
        {
            given_context();
            given_request_stream("Customer.FirstName=John&Customer.FirstName=Jack&Customer.LastName=Doe");

            when_decoding<Customer>();
            then_decoding_result<Customer>().FirstName.LegacyShouldBe(null);
            then_decoding_result<Customer>().LastName.LegacyShouldBe("Doe");

        }
        [Test]
        public void the_complex_type_is_built_from_key_value_pairs()
        {
            given_context();
            given_request_stream("Customer.FirstName=John&Customer.LastName=Doe&Customer.DateOfBirth.Day=10");

            when_decoding<Customer>();

            then_decoding_result<Customer>().ShouldBeAssignableTo<Customer>();

            then_decoding_result<Customer>().FirstName
                    .LegacyShouldBe("John");
            then_decoding_result<Customer>().LastName
                    .LegacyShouldBe("Doe");

            then_decoding_result<Customer>().DateOfBirth.Day
                    .LegacyShouldBe(10);
        }
        [Test]
        public void the_changeset_type_is_built_from_key_value_pairs()
        {
            given_context();
            given_request_stream("Customer.FirstName=John&Customer.LastName=Doe&Customer.DateOfBirth.Day=10");

            when_decoding<ChangeSet<Customer>>();

            var customer = new Customer();
            then_decoding_result<ChangeSet<Customer>>()
                .Apply(customer);

            customer.FirstName
                    .LegacyShouldBe("John");
            customer.LastName
                      .LegacyShouldBe("Doe");

            customer.DateOfBirth.Day
                    .LegacyShouldBe(10);
        }
        [Test]
        public void indexers_are_supported_when_encoded()
        {
            given_context();
            given_request_stream("Customer.Attributes%3A1=blue&Customer.Attributes%3A2=red");

            when_decoding<Customer>();

            then_decoding_result<Customer>()
                .Attributes.Count().LegacyShouldBe(2);
        }


    }
    public class app_www_context : media_type_reader_context<ApplicationXWwwFormUrlencodedObjectCodec>
    {

        protected override ApplicationXWwwFormUrlencodedObjectCodec CreateCodec(ICommunicationContext context)
        {
            return new ApplicationXWwwFormUrlencodedObjectCodec(context, new DefaultObjectBinderLocator());
        }
    }
    public class Customer
    {
        public Customer() { }
        public Customer(string firstname) { FirstName = firstname; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<string> Attributes { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Address Address { get; set; }
    }
    public class Address
    {
        public string Line1 { get; set; }   
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
