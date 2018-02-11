namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxiedHandler
  {
    public string Get()
    {
      return "OK";
    }

    public string Get(string query)
    {
      return query;
    }
  }
}