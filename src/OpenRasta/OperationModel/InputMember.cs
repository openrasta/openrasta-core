using OpenRasta.Binding;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel
{
  public class InputMember
  {
    public InputMember(IMember member, IObjectBinder binder, bool isOptional)
    {
      Member = member;
      Binder = binder;
      IsOptional = isOptional;
    }

    public IObjectBinder Binder { get; }
    public bool IsOptional { get; }
    public bool IsReadyForAssignment => IsOptional || !Binder.IsEmpty;
    public IMember Member { get; private set; }
  }
}