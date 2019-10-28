using System;
using System.Linq.Expressions;

namespace OpenRasta.Configuration
{
  public interface ITypeRegistrationContext
  {
    ITypeRegistrationOptions<TConcrete> Singleton<TConcrete>();
    ITypeRegistrationOptions<TConcrete> Singleton<TConcrete>(Expression<Func<TConcrete>> factory);
    ITypeRegistrationOptions<TConcrete> Singleton<TArg1, TConcrete>(Expression<Func<TArg1, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> Singleton<TArg1, TArg2, TConcrete>(
      Expression<Func<TArg1, TArg2, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> Singleton<TArg1, TArg2, TArg3, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> Singleton<TArg1, TArg2, TArg3, TArg4, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TArg4, TConcrete>> factory);


    ITypeRegistrationOptions<TConcrete> Transient<TConcrete>(Expression<Func<TConcrete>> factory);
    ITypeRegistrationOptions<TConcrete> Transient<TArg1, TConcrete>(Expression<Func<TArg1, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> Transient<TArg1, TArg2, TConcrete>(
      Expression<Func<TArg1, TArg2, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> Transient<TArg1, TArg2, TArg3, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> Transient<TArg1, TArg2, TArg3, TArg4, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TArg4, TConcrete>> factory);


    ITypeRegistrationOptions<TConcrete> PerRequest<TConcrete>(Expression<Func<TConcrete>> factory);
    ITypeRegistrationOptions<TConcrete> PerRequest<TArg1, TConcrete>(Expression<Func<TArg1, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> PerRequest<TArg1, TArg2, TConcrete>(
      Expression<Func<TArg1, TArg2, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> PerRequest<TArg1, TArg2, TArg3, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TConcrete>> factory);

    ITypeRegistrationOptions<TConcrete> PerRequest<TArg1, TArg2, TArg3, TArg4, TConcrete>(
      Expression<Func<TArg1, TArg2, TArg3, TArg4, TConcrete>> factory);
  }
}