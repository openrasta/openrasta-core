using Shouldly;
using Xunit;

namespace Tests.DI.SpeedyGonzales
{
  public class transient
  {
    SpeedyGonzalesResolver container;

    public transient()
    {
      container = new SpeedyGonzalesResolver
      {
        {context => context.Transient(() => new SimpleService())}
      };
      container.Seal();
    }

    [Fact]
    public void is_resolved()
    {
      var instance1 = container.Resolve<SimpleService>();
      var instance2 = container.Resolve<SimpleService>();
      instance1.ShouldNotBeSameAs(instance2);
    }
  }
}