using System.Runtime.InteropServices;
using OpenRasta.DI;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased
{
  public class MockOperationHandler
  {
    public IDependencyResolver Dependency { get; set; }
    public int Get([Optional] int index)
    {
      return index;
    }
    public object Post(int index, string value)
    {
      return null;
    }
    public object Search([Optional, DefaultParameterValue("*")] string searchString)
    {
      return 0;
    }

    public object SearchNative(string searchString = "*")
    {
      return 0;
    }
  }
}
