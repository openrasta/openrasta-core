JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
#region License

/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using OpenRasta.Web;
using OpenRasta.Web.Pipeline;

namespace OpenRasta.Security
{
    public class PrincipalAuthorizationAttribute : RequiresAuthenticationAttribute
    {
        public string[] InRoles { get; set; }
        public string[] Users { get; set; }

        public override PipelineContinuation ExecuteBefore(ICommunicationContext context)
        {
            if ((InRoles == null || InRoles.Length == 0) && (Users == null || Users.Length == 0))
                return PipelineContinuation.Continue;
            if (base.ExecuteBefore(context) == PipelineContinuation.Continue)
            {
                try
                {
                    if (InRoles != null)
                        foreach (string role in InRoles)
                            if (context.User.IsInRole(role))
                                return PipelineContinuation.Continue;
                    if (Users != null)
                        foreach (string user in Users)
                            if (context.User.Identity.Name == user)
                                return PipelineContinuation.Continue;
                }
                catch
                {
                    // todo: decide where to log this error.
                }
            }
            context.OperationResult = new OperationResult.Unauthorized();
            return PipelineContinuation.RenderNow;
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