using System;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
    public class ThrowHandler
    {
        public object Get()
        {
            throw new NotImplementedException();
        }
        [HttpOperation(ForUriName="success")]
        public object GetSuccessful()
        {
            return "hello";
        }
    }
}