using System.Threading.Tasks;
using OpenRasta.DI;
using Shouldly;
using Xunit;

namespace Tests.DI
{
  public class declaration_on_task_access_on_continuation
  {
    [Fact]
    public async Task is_accessible()
    {
      var container = new InternalDependencyResolver();
      IDependencyResolver containerOnTask = null;
      Task.Run(() =>
      {
        DependencyManager.SetResolver(container);
        Task.Run(() => { containerOnTask = DependencyManager.Current; }).Wait();
      }).Wait();

      containerOnTask.ShouldBeSameAs(container);
      DependencyManager.Current.ShouldBeNull();
    }
  }
}