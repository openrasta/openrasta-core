using System;
using System.Data;

namespace OpenRasta.Configuration
{
  class TypeRegistrationOptions<TConcrete> : ITypeRegistrationOptions<TConcrete>
  {
    readonly TypeRegistrationContext _context;

    public TypeRegistrationOptions(TypeRegistrationContext context)
    {
      _context = context;
    }

    public ITypeRegistrationOptions<TConcrete> As<T>()
    {
      if (typeof(T).IsAssignableFrom(typeof(TConcrete)) == false)
        throw new InvalidOperationException($"Type {typeof(TConcrete)} is not assignable to {typeof(T)}");
          
      _context.Model.ServiceType = typeof(T);
      return this;
    }
  }
}