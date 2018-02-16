using System;
using System.Collections.Generic;
using OpenRasta.Binding;

namespace OpenRasta.Plugins.ReverseProxy
{
  [Binder(Type = typeof(AnyParameters))]
  public class AnyParameters : IObjectBinder
  {
    public bool IsEmpty { get; } = false;
    public ICollection<string> Prefixes { get; } = Array.Empty<string>();
    public bool SetProperty<TValue>(string key, IEnumerable<TValue> values, ValueConverter<TValue> converter) => true;
    public bool SetInstance(object builtInstance) => false;
    public BindingResult BuildObject() => BindingResult.Success(null);
  }
}