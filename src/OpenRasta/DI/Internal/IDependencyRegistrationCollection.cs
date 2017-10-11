using System;
using System.Collections.Generic;

namespace OpenRasta.DI.Internal
{
  public interface IDependencyRegistrationCollection
  {
    IEnumerable<DependencyRegistration> this[Type serviceType] { get; }
    void Add(DependencyRegistration registration);
    bool HasRegistrationForService(Type type);
    DependencyRegistration DefaultRegistrationFor(Type serviceType);
  }
}