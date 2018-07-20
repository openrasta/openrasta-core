using OpenRasta.Plugins.Hydra;

namespace Tests.Plugins.Hydra.Examples
{
  public class Event
  {
    
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