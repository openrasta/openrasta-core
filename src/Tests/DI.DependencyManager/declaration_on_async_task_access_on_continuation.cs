using System.Threading.Tasks;
using OpenRasta.DI;
using Shouldly;
using Xunit;

namespace Tests.DI
{
  public class declaration_on_async_task_access_on_continuation
  {
    [Fact]
    public async Task is_accessible()
    {
      var container = new InternalDependencyResolver();
      IDependencyResolver containerOnTask = null;
      await Task.Run(async () =>
      {
        DependencyManager.SetResolver(container);
        await Task.Run(() => { containerOnTask = DependencyManager.Current; });
      });

      containerOnTask.ShouldBeSameAs(container);
      DependencyManager.Current.ShouldBeNull();
    }
  }
}