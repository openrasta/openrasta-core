using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using OpenRasta.Web;

namespace OpenRasta.DI.Windsor
{
  class ScopedInstanceStore
  {
    readonly Dictionary<Type, object> _store = new Dictionary<Type, object>(0);
    public ICommunicationContext Context { get; set; }

    public object GetInstance(Type serviceType)
    {
      return _store.TryGetValue(serviceType, out var service)
        ? service
        : throw new ComponentNotFoundException(serviceType);
    }

    public void SetInstance(Type serviceType, object instance)
    {
      _store[serviceType] = instance;
    }
  }
}