using Shouldly;
using Xunit;

namespace Tests.DI.SpeedyGonzales
{
  public class dependency
  {
    SpeedyGonzalesResolver container;

    public dependency()
    {
      container = new SpeedyGonzalesResolver
      {
        context => context.Transient((Hero hero) => new Quest(hero,null)),
        context => context.Transient(() => new Hero("Heracles"))
      };
      container.Seal();
    }

    [Fact]
    public void is_resolved()
    {
      var firstQuest = container.Resolve<Quest>();
      var secondQuest = container.Resolve<Quest>();
      var hero = container.Resolve<Hero>();
      firstQuest.ShouldNotBeSameAs(secondQuest);
      firstQuest.Hero.ShouldNotBeSameAs(hero);
      secondQuest.Hero.ShouldNotBeSameAs(hero);
    }
  }
}