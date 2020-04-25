using System.Threading.Tasks;
using OpenRasta.Configuration;
using Shouldly;

namespace Tests.Scenarios.HandlerInheritance
{
  public class methods_on_abstract_base_class
  {
    abstract class AnimalHandler
    {
      public string Get() => "OK";
    }

    class CatHandler : AnimalHandler
    {
    }

    [GitHubIssue(210)]
    public async Task gets_invoked()
    {
      var server = new TestHost((has, uses) =>
        has.ResourcesNamed("cat")
          .AtUri("/cat")
          .HandledBy<CatHandler>());

      (await server.Get("/cat")).StatusCode.ShouldBe(200);
    }
  }
}