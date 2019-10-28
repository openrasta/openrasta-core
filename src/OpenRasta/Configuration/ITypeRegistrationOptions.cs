namespace OpenRasta.Configuration
{
  public interface ITypeRegistrationOptions<TConcrete>
  { 
    ITypeRegistrationOptions<TConcrete> As<TService>();
  }
}