namespace Tests.Scenarios.HandlerSelection
{
  public class CollectionHandler<TId,TContent>
  {
    public string Post(TId account_id, TContent content)
    {
      return "POST:" + account_id;
    }

    public string Post(string content)
    {
      return "POST";
    }
  }
}