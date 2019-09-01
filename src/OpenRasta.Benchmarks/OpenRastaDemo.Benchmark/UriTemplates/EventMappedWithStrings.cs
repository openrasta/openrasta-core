namespace OpenRastaDemo.Benchmark.UriTemplates
{
  public class EventMappedWithStrings
  {
    public EventMappedWithStrings(int id)
    {
      Name = $"Event with id {id}";
      Id = id;
    }

    public int Id { get; set; }

    public string Name { get; }

    public class Handler
    {
      public EventMappedWithStrings Get(int id) => new EventMappedWithStrings(id);
    }
  }
}