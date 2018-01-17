using System;
using OpenRasta.DI;

namespace OpenRasta.Configuration.MetaModel
{
    public class DependencyRegistrationModel
    {
        public DependencyRegistrationModel(Type serviceType, Type concreteType, DependencyLifetime lifetime)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (concreteType == null) throw new ArgumentNullException(nameof(concreteType));
            ServiceType = serviceType;
            ConcreteType = concreteType;
            Lifetime = lifetime;
        }

        public Type ConcreteType { get; }
        public DependencyLifetime Lifetime { get; }
        public Type ServiceType { get; }
    }

}