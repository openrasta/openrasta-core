using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Security
{
    public class RequiresBasicAuthenticationInterceptor : RequiresAuthenticationInterceptor
    {
        private readonly string realm;

        public RequiresBasicAuthenticationInterceptor(ICommunicationContext context, string realm)
            : base(context)
        {
            this.realm = realm;
        }

        protected override void DenyAuthorization(ICommunicationContext context)
        {
            base.DenyAuthorization(context);
            var header = new BasicAuthenticationRequiredHeader(realm).ServerResponseHeader;
            context.Response.Headers["WWW-Authenticate"] = header;
        }
    }
}
