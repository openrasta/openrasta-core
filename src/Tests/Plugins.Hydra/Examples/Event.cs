using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Schemas;

namespace Tests.Plugins.Hydra.Examples
{
  public class Event : JsonLd.INode
  {
    public int Id { get; set; }
  }

  public class Customer
  {
    public string Name { get; set; }
  }

  public static class ExampleVocabularies
  {
    public static readonly Vocabulary ExampleApp = new Vocabulary("https://openrasta.example/schemas/events#", "ev");
  }
}