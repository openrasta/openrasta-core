using System;
using System.Collections.Generic;
using OpenRasta.DI;

namespace OpenRasta.Configuration.MetaModel
{
  public abstract class DependencyFactoryModel
  {
    public readonly IEnumerable<Type> Arguments;
    public Delegate Factory;
    public Func<object[], object> Invoker;

    public DependencyFactoryModel(Type concreteType, Delegate factory = null, params Type[] args)
    {
      Arguments = args;
      Factory = factory;
      ServiceType = ConcreteType = concreteType;
    }

    public Type ServiceType { get; set; }
    public DependencyLifetime Lifetime { get; set; }
    public Type ConcreteType { get; }
  }

  public class DependencyFactoryModel<TConcrete> : DependencyFactoryModel
  {
    public DependencyFactoryModel() : base(typeof(TConcrete))
    {
    }
    public DependencyFactoryModel(Func<TConcrete> factory)
      : base(typeof(TConcrete), factory)
    {
      Invoker = args => factory();
    }
  }
  public class DependencyFactoryModel<TArg1,TConcrete> : DependencyFactoryModel
  {
    public DependencyFactoryModel(Func<TArg1,TConcrete> factory)
      : base(typeof(TConcrete), factory, typeof(TArg1))
    {
      Invoker = args => factory((TArg1)args[0]);
    }
  }
  public class DependencyFactoryModel<TArg1,TArg2,TConcrete> : DependencyFactoryModel
  {
    public DependencyFactoryModel(Func<TArg1,TArg2,TConcrete> factory)
      : base(typeof(TConcrete), factory, typeof(TArg1), typeof(TArg2))
    {
      Invoker = args => factory((TArg1)args[0],(TArg2)args[1]);
    }
  }
  public class DependencyFactoryModel<TArg1,TArg2,TArg3,TConcrete> : DependencyFactoryModel
  {
    public DependencyFactoryModel(Func<TArg1,TArg2,TArg3,TConcrete> factory)
      : base(typeof(TConcrete), factory, typeof(TArg1), typeof(TArg2), typeof(TArg3))
    {
      Invoker = args => factory((TArg1)args[0], (TArg2)args[1], (TArg3)args[2]);
    }
  }
  public class DependencyFactoryModel<TArg1,TArg2,TArg3,TArg4,TConcrete> : DependencyFactoryModel
  {
    public DependencyFactoryModel(Func<TArg1,TArg2,TArg3,TArg4, TConcrete> factory)
      : base(typeof(TConcrete), factory, typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4))
    {
      Invoker = args => factory((TArg1)args[0], (TArg2)args[1], (TArg3)args[2], (TArg4)(args[3]));
    }
  }
}