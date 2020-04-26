namespace Tests.Scenarios.HandlerSelection
{
  public class CollectionHandler<TId,TContent>
  {
    public string Get()
    {
      return "GET";
    }
    public string Get(TId account_id)
    {
      return $"GET:{account_id}";
    }
    public string Post(TId account_id, TContent content)
    {
      return $"POST:{account_id}:{content}";
    }

    public string Post(string content)
    {
      return $"POST:{content}";
    }
  }
}