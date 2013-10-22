#region License
/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using System.Security.Principal;
using System.Threading;
using Moq;
using NUnit.Framework;
using OpenRasta.Hosting.InMemory;
using OpenRasta.OperationModel;
using OpenRasta.Security;
using OpenRasta.Testing;

namespace PrincipalAuthorizationResolver_Specification
{
    public class when_the_user_is_not_authenticated : context
    {
        [Test, Ignore]
        public void the_filter_doesnt_authorize_the_execution()
        {
            var context = new InMemoryCommunicationContext();
            var principal = new PrincipalAuthorizationInterceptor(context) { InRoles = new[] { "Administrators"}};

            principal.BeforeExecute(new Mock<IOperation>().Object)
                .ShouldBe(true);
        }
    }

    [TestFixture]
    public class when_the_user_is_authenticated : context
    {
        [Test, Ignore]
        public void the_role_is_matched_and_execution_continues()
        {
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("name"), new[] {"Administrator"});

            var rastaContext = new InMemoryCommunicationContext();
            var principal = new PrincipalAuthorizationInterceptor(rastaContext) { InRoles = new[] { "Administrators" } };

            principal.BeforeExecute(new Mock<IOperation>().Object)
                .ShouldBe(true);


            rastaContext.OperationResult.ShouldBeNull();
        }

        [Test, Ignore]
        public void the_username_is_matched_and_execution_continues()
        {
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("johndoe"), new[] { "Administrator" });

            var rastaContext = new InMemoryCommunicationContext();
            var authorizer = new PrincipalAuthorizationInterceptor(rastaContext) { Users = new[] { "johndoe" } };
            authorizer.BeforeExecute(new Mock<IOperation>().Object)
              .ShouldBe(true);

            rastaContext.OperationResult.ShouldBeNull();
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