#region License

/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;

namespace OpenRasta.Security
{
    //todo: modified to get this to compile - assumed that this was intent; but this code is not production ready wihtout review
    public class PrincipalAuthorizationAttribute : InterceptorProviderAttribute
    {
         public override IEnumerable<IOperationInterceptor> GetInterceptors(IOperation operation)
        {
            return new[]
            {
                new PrincipalAuthorizationInterceptor(DependencyManager.GetService<ICommunicationContext>())
            };
        }
    }

    public class PrincipalAuthorizationInterceptor: OperationInterceptor
    {
        public string[] InRoles { get; set; }
        public string[] Users { get; set; }
        readonly ICommunicationContext _context;

        public PrincipalAuthorizationInterceptor(ICommunicationContext context)
        {
            _context = context;
        }

        public override bool BeforeExecute(IOperation operation)
        {
            if ((InRoles == null || InRoles.Length == 0) && (Users == null || Users.Length == 0))
                return true;
            
            try
            {
                if (InRoles != null)
                    if (InRoles.Any(role => _context.User.IsInRole(role)))
                    {
                        return true;
                    }

                if (Users != null)
                    if (Users.Any(user => _context.User.Identity.Name == user))
                    {
                        return true;
                    }
            }
            catch
            {
                // todo: decide where to log this error.
            }

            _context.OperationResult = new OperationResult.Unauthorized();
            return false;
        }
    }
}

#region Full license

// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion