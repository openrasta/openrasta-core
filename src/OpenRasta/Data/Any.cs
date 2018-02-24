using System;
using System.Collections.Generic;
using OpenRasta.Binding;

namespace OpenRasta.Data
{
  [Binder(Type = typeof(Any))]
  public class Any : IObjectBinder
  {
    public bool IsEmpty { get; } = false;
    public ICollection<string> Prefixes { get; } = Array.Empty<string>();
    public bool SetProperty<TValue>(string key, IEnumerable<TValue> values, ValueConverter<TValue> converter) => true;
    public bool SetInstance(object builtInstance) => false;
    public BindingResult BuildObject() => BindingResult.Success(null);
  }
}