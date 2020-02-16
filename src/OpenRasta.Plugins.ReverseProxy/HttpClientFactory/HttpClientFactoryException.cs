using System;

namespace OpenRasta.Plugins.ReverseProxy.HttpClientFactory
{
  public class HttpClientFactoryException : Exception
  {
    public HttpClientFactoryException(string message)
      : base(message)
    {
    }
  }
}