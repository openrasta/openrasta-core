using System.IO;
using OpenRasta.Tests.Unit.Fakes;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public class HandlerRequiringInputs
  {
    public string PostName(Frodo frodo) => null;

    public string PostAddress(Frodo frodo, Address address) => null;

    public string PostStream(Stream anObject) => null;

    public string PostTwo(Frodo frodo1, Frodo frodo2) => null;
  }
}
