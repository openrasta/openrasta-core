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
using System.Globalization;
using OpenRasta.Codecs;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Testing;
using NUnit.Framework;
using OpenRasta.Web;

namespace JsonpDataContractCodec_Specification
{
    public class when_writing_a_jsonp_representation : context
    {
        [Test]
        public void the_content_is_written_to_the_stream()
        {
            var codec = new JsonpDataContractCodec(new DummyRequest(new Uri("http://dave.com/something?jsoncallback=123456789")));
            var stub = new InMemoryResponse();
            codec.WriteTo(new Customer { Name = "hello" }, stub.Entity, null);

            stub.Entity.Stream.Position
                .ShouldNotBe(0);
        }
    }

    public class when_writing_a_json_representation : context
    {
        [Test]
        public void the_content_is_written_to_the_stream()
        {
            var codec = new JsonpDataContractCodec(new DummyRequest(new Uri("http://dave.com/something")));
            var stub = new InMemoryResponse();
            codec.WriteTo(new Customer { Name = "hello" }, stub.Entity, null);

            stub.Entity.Stream.Position
                .ShouldNotBe(0);
        }
    }

    public class Customer { public string Name { get; set; } }
    internal class DummyRequest : IRequest
    {
        readonly Uri _uriToReturn;

        public DummyRequest(Uri uriToReturn)
        {
            _uriToReturn = uriToReturn;
        }

        public IHttpEntity Entity
        {
            get { throw new NotImplementedException(); }
        }

        public HttpHeaderDictionary Headers
        {
            get { throw new NotImplementedException(); }
        }

        public Uri Uri
        {
            get { return _uriToReturn; }
            set { throw new NotImplementedException(); }
        }

        public string UriName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public CultureInfo NegotiatedCulture
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string HttpMethod
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IList<string> CodecParameters
        {
            get { throw new NotImplementedException(); }
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
