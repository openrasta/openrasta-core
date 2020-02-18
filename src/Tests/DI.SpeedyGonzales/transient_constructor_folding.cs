using Xunit;

namespace Tests.DI.SpeedyGonzales
{
  public class transient_constructor_folding
  {
    [Fact]
    public void transient_constructors_get_rewritten()
    {
      var container = new SpeedyGonzalesResolver
      {
        {context => context.Transient((Hero hero) => new Quest(hero,null))},
        {context => context.Transient(() => new Hero("Heracles"))}
      };
      container.Seal();
    }
  }
}