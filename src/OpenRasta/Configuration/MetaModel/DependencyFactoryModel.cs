using System;
using System.Collections.Generic;
using OpenRasta.DI;

namespace OpenRasta.Configuration.MetaModel
{
  public abstract class DependencyFactoryModel
  {
    public IEnumerable<Type> Arguments;
    public Delegate Factory;
    public Func<object[], object> Invoker;
    public DependencyFactoryModel(Delegate factory, Type concreteType, params Type[] args)
    {
      Arguments = args;
      Factory = factory;
      ServiceType = ConcreteType = concreteType;
    }

    public Type ServiceType { get; set; }
    public DependencyLifetime Lifetime { get; set; }
    public Type ConcreteType { get; }
  }
  public class DependencyFactoryModel<T> : DependencyFactoryModel
  {
    public DependencyFactoryModel(Func<T> factory)
      : base(factory, typeof(T))
    {
      Invoker = args => factory();
    }
  }
}