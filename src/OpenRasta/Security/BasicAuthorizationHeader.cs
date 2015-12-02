#region License

/* Authors:
 *      Dylan Beattie (dylan@dylanbeattie.net)
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2015 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */

#endregion

using System;
using System.Text;

namespace OpenRasta.Security
{
    public class BasicAuthorizationHeader
    {
        private BasicAuthorizationHeader(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }

        public static BasicAuthorizationHeader Parse(string header)
        {
            var tokens = header.Split(' ');
            if (tokens.Length != 2)
            {
                throw (new ArgumentException("Supplied header is not in the format Basic {base64-encoded credential pair}", "header"));
            }
            if (tokens[0] != "Basic")
            {
                throw (new ArgumentException("Supplied header is not an HTTP Basic authorization header", header));
            }
            try
            {
                var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(tokens[1]));

                var credentials = credentialString.Split(new[] { ':' }, 2);
                return (new BasicAuthorizationHeader(credentials[0], credentials[1]));
            } catch (FormatException ex)
            {
                throw (new ArgumentException("Supplied header doesn't contain valid base64 encoded credentials", ex));
            }
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
