using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
    public class CustomerHandler
    {
        public OperationResult Get(string customerId)
        {
            return new OperationResult.OK();
        }

        public OperationResult Patch(int customerId, string customerName)
        {
            return new OperationResult.OK
            {
                ResponseResource = customerName, 
                RedirectLocation = new Customer { CustomerID = customerId }.CreateUri()
            };
        }
    }
}