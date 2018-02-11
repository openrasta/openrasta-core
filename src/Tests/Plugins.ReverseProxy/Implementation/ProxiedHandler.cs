namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxiedHandler
  {
    public string Get()
    {
      return "empty";
    }

    public string Get(string query)
    {
      return query;
    }
  }
}