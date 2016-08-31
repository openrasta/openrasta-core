using System;
using System.Text;
using OpenRasta;

namespace OpenRasta.Authentication.Basic
{
    [Obsolete("Authentication features are moving to a new package, see more information at http://https://github.com/openrasta/openrasta/wiki/Authentication")]
    public class BasicAuthRequestHeader
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public BasicAuthRequestHeader(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}