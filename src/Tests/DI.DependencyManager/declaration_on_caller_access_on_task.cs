using System.Threading.Tasks;
using OpenRasta.DI;
using Shouldly;
using Xunit;

namespace Tests.DI
{
  public class declaration_on_caller_access_on_task
  {
    [Fact]
    public async Task is_accessible()
    {
      var container = new InternalDependencyResolver();
      DependencyManager.SetResolver(container);
      IDependencyResolver containerOnTask = null;
      await Task.Run(async () =>
      {
        containerOnTask = DependencyManager.Current;
      });
      
      containerOnTask.ShouldBeSameAs(container);
      DependencyManager.UnsetResolver();
      DependencyManager.Current.ShouldBeNull();
    }
  }
}