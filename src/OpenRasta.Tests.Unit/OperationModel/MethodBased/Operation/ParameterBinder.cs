using System;
using System.Collections.Generic;
using OpenRasta.Binding;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  public class ParameterBinder : IObjectBinder
  {
    public bool IsEmpty { get; private set; }
    public ICollection<string> Prefixes { get; private set; }
    public bool SetProperty<TValue>(string key, IEnumerable<TValue> values, ValueConverter<TValue> converter)
    {
      throw new NotImplementedException();
    }

    public bool SetInstance(object builtInstance)
    {
      throw new NotImplementedException();
    }

    public BindingResult BuildObject()
    {
      throw new NotImplementedException();
    }
  }
}
