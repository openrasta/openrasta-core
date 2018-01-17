using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace Tests.Configuration.Dependencies
{
  class ClassWithArguments
  {
    public ClassWithArguments(params ClassWithDefaultConstructor[] first)
    {
      Dependencies = first.Cast<object>().ToList();
    }


    public List<object> Dependencies { get; set; }

    public void ShouldHaveDependencies(int count)
    {
      Dependencies.Count.ShouldBe(count);
      foreach (var arg in Dependencies)
        arg.ShouldBeOfType<ClassWithDefaultConstructor>();
    }
  }
}