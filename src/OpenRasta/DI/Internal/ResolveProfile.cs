using System;

namespace OpenRasta.DI.Internal
{
  abstract class ResolveProfile
  {
    public abstract bool TryResolve(IDependencyRegistrationCollection registrations, ResolveContext resolveContext, out object instance);
    
  }
}