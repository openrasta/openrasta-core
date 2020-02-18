using Shouldly;
using Xunit;

namespace Tests.DI.SpeedyGonzales
{
  public class singleton
  {
    SpeedyGonzalesResolver container;

    public singleton()
    {
      container = new SpeedyGonzalesResolver
      {
        {context => context.Singleton(() => new SimpleService())}
      };
      container.Seal();
    }

    [Fact]
    public void is_resolved()
    {
      var instance1 = container.Resolve<SimpleService>();
      var instance2 = container.Resolve<SimpleService>();
      instance1.ShouldBeSameAs(instance2);
    }
  }
}