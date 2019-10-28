using System;
using System.Linq.Expressions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;

namespace OpenRasta.Configuration
{
  class TypeRegistrationContext : ITypeRegistrationContext
  {
    public ITypeRegistrationOptions<TConcrete> Singleton<TConcrete>()
    {
      Model = new DependencyFactoryModel<TConcrete> {Lifetime = DependencyLifetime.Singleton};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Singleton<TConcrete>(Expression<Func<TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TConcrete>(factory) {Lifetime = DependencyLifetime.Singleton};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Singleton<TArg1, TConcrete>(Expression<Func<TArg1, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TConcrete>(factory) {Lifetime = DependencyLifetime.Singleton};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Singleton<TArg1, TArg2, TConcrete>(
      Expression<Func<TArg1, TArg2, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TConcrete>(factory) {Lifetime = DependencyLifetime.Singleton};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Singleton<TArg1, TArg2, TArg3, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TArg3, TConcrete>(factory)
      {
        Lifetime = DependencyLifetime.Singleton
      };
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Singleton<TArg1, TArg2, TArg3, TArg4, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TArg4, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TArg3, TArg4, TConcrete>(factory)
      {
        Lifetime = DependencyLifetime.Singleton
      };
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Transient<TConcrete>()
    {
      Model = new DependencyFactoryModel<TConcrete>() {Lifetime = DependencyLifetime.Transient};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Transient<TConcrete>(Expression<Func<TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TConcrete>(factory) {Lifetime = DependencyLifetime.Transient};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Transient<TArg1, TConcrete>(Expression<Func<TArg1, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TConcrete>(factory) {Lifetime = DependencyLifetime.Transient};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Transient<TArg1, TArg2, TConcrete>(
      Expression<Func<TArg1, TArg2, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TConcrete>(factory) {Lifetime = DependencyLifetime.Transient};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Transient<TArg1, TArg2, TArg3, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TArg3, TConcrete>(factory)
      {
        Lifetime = DependencyLifetime.Transient
      };
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> Transient<TArg1, TArg2, TArg3, TArg4, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TArg4, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TArg3, TArg4, TConcrete>(factory)
      {
        Lifetime = DependencyLifetime.Transient
      };
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> PerRequest<TConcrete>()
    {
      Model = new DependencyFactoryModel<TConcrete>() {Lifetime = DependencyLifetime.PerRequest};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> PerRequest<TConcrete>(Expression<Func<TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TConcrete>(factory) {Lifetime = DependencyLifetime.PerRequest};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> PerRequest<TArg1, TConcrete>(Expression<Func<TArg1, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TConcrete>(factory) {Lifetime = DependencyLifetime.PerRequest};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> PerRequest<TArg1, TArg2, TConcrete>(
      Expression<Func<TArg1, TArg2, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TConcrete>(factory) {Lifetime = DependencyLifetime.PerRequest};
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> PerRequest<TArg1, TArg2, TArg3, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TArg3, TConcrete>(factory)
      {
        Lifetime = DependencyLifetime.PerRequest
      };
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public ITypeRegistrationOptions<TConcrete> PerRequest<TArg1, TArg2, TArg3, TArg4, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TArg4, TConcrete>> factory)
    {
      Model = new DependencyFactoryModel<TArg1, TArg2, TArg3, TArg4, TConcrete>(factory)
      {
        Lifetime = DependencyLifetime.PerRequest
      };
      return new TypeRegistrationOptions<TConcrete>(this);
    }

    public DependencyFactoryModel Model { get; set; }
  }
}