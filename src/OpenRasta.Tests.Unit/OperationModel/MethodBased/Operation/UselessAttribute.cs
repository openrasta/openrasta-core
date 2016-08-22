using System;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited=true)]
  public class UselessAttribute : Attribute, IUseless
  {
    public string Name { get; set; }

    public UselessAttribute(string name)
    {
      Name = name;
    }
  }
}
