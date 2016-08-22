using System.ComponentModel;
using OpenRasta.Binding;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  [Useless("type attribute")]
  public class OperationHandlerForAttributes
  {
    [Description("Description")]
    public void GetHasOneAttribute(int index){}
    [Useless("one")]
    [Useless("two")]
    public void GetHasTwoAttributes(int index){}
    public void GetHasParameterAttribute([Binder(Type=typeof(ParameterBinder))]int index) { }
  }
}
