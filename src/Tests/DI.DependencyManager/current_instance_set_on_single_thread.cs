using System.Threading.Tasks;
using OpenRasta.DI;
using Shouldly;
using Xunit;

namespace Tests.DI
{
  public class current_instance_set_on_single_thread
  {
    [Fact]
    public void is_accessibe()
    {
      var container = new InternalDependencyResolver();
      DependencyManager.SetResolver(container);
      DependencyManager.Current.ShouldBeSameAs(container);
      DependencyManager.UnsetResolver();
      DependencyManager.Current.ShouldBeNull();
    }
  }
}