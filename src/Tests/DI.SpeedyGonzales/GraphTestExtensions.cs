using Shouldly;

namespace Tests.DI.SpeedyGonzales
{
  public static class GraphTestExtensions
  {
    public static void ShouldBe<TService>(this GraphNode node, string factory)
    {
      node.Model.ServiceType.ShouldBe(typeof(TService));
      node.FactoryExpression.ToString().ShouldBe(factory);
    }
  }
}