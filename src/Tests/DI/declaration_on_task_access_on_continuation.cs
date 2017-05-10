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
      await Task.Run(() => containerOnTask = DependencyManager.Current).ContinueWith(
        previous => {
          DependencyManager.UnsetResolver();
        }
      );
      containerOnTask.ShouldBeSameAs(container);

      DependencyManager.Current.ShouldBeNull();
    }
  }

  public class declaration_on_task_access_on_continuation
  {
    [Fact]
    public async Task is_accessible()
    {
      var container = new InternalDependencyResolver();
      IDependencyResolver containerOnTask = null;
        IDependencyResolver containerAfterUnset = null;
      await Task.Run(async () =>
      {
        DependencyManager.SetResolver(container);
        await Task.Run(() => { containerOnTask = DependencyManager.Current; });
        await Task.Run(() => DependencyManager.UnsetResolver());
        containerAfterUnset = DependencyManager.Current;
      });

      containerOnTask.ShouldBeSameAs(container);
      containerAfterUnset.ShouldBeNull();

      DependencyManager.Current.ShouldBeNull();
    }
  }
}