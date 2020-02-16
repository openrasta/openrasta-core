using OpenRasta.Web;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyOperationResult : OperationResult
  {
    public ReverseProxyOperationResult(ReverseProxyResponse response)
      :base(response.StatusCode)
    {
      ResponseResource = response;
    }
    public static implicit operator ReverseProxyOperationResult(ReverseProxyResponse response)
    {
      return new ReverseProxyOperationResult(response);
    }
  }
}