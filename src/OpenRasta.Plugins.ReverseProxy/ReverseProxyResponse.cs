using System;
using System.Net.Http;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyResponse
  {
    public HttpRequestMessage RequestMessage { get; }
    public HttpResponseMessage ResponseMessage { get; }
    public int StatusCode { get; }
    public Exception Error { get; }

    public ReverseProxyResponse(
      HttpRequestMessage requestMessage,
      HttpResponseMessage responseMessage = null,
      Exception error = null,
      int? statusCode = null)
    {
      RequestMessage = requestMessage ?? throw new ArgumentNullException(nameof(requestMessage));
      ResponseMessage = responseMessage;
      StatusCode = statusCode ?? (responseMessage != null ? (int)responseMessage.StatusCode : 500);
    }

    public void Dispose()
    {
      RequestMessage.Dispose();
      ResponseMessage?.Dispose();  
    }
  }
}