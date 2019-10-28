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
      _context.Model.ServiceType = typeof(T);
      return this;
    }
  }
}