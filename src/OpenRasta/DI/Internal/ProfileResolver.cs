namespace OpenRasta.DI.Internal
{
  delegate bool ProfileResolver(
    IDependencyRegistrationCollection registrations, 
    ResolveContext currentContext,
    out object instance);
}