using System;
using System.ComponentModel;
using OpenRasta.Codecs;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Implementation;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration
{
    public static class HasExtensions
    {
        public static IResourceDefinition ResourcesNamed(this IHas has, string name)
        {
          var definition = has.ResourcesWithKey(name);
          definition.Resource.Name = name;
          return definition;
        }
        
        public static IResourceDefinition<TResource> ResourcesOfType<TResource>(this IHas has)
        {

            if (has == null) throw new ArgumentNullException("has");

            var hasBuilder = (IFluentTarget)has;

            ResourceModel registration = RegisterResourceModel(hasBuilder, typeof(TResource));

            return new ResourceDefinition<TResource>(hasBuilder, hasBuilder.TypeSystem, registration);
        }

        public static IResourceDefinition ResourcesOfType(this IHas has, Type clrType)
        {
            return has.ResourcesWithKey(clrType);
        }

        public static IResourceDefinition ResourcesOfType(this IHas has, IType type)
        {
            return has.ResourcesWithKey(type);
        }

        /// <exception cref="ArgumentNullException"><c>has</c> is null.</exception>
        public static IResourceDefinition ResourcesWithKey(this IHas has, object resourceKey)
        {
            if (has == null) throw new ArgumentNullException("has");
            if (resourceKey == null) throw new ArgumentNullException("resourceKey");

            var hasBuilder = (IFluentTarget)has;

            ResourceModel registration = RegisterResourceModel(hasBuilder, resourceKey);

            return new ResourceDefinition(hasBuilder, hasBuilder.TypeSystem, registration);
        }

        static ResourceModel RegisterResourceModel(IFluentTarget has, object resourceKey)
        {
            var resourceKeyAsType = resourceKey as Type;
            bool isStrictRegistration = false;
            if (resourceKeyAsType != null && CodecRegistration.IsStrictRegistration(resourceKeyAsType))
            {
                resourceKey = CodecRegistration.GetStrictType(resourceKeyAsType);
                isStrictRegistration = true;
            }
            var registration = new ResourceModel
            {
                    ResourceKey = resourceKey, 
                    IsStrictRegistration = isStrictRegistration
            };
            has.Repository.ResourceRegistrations.Add(registration);
            return registration;
        }
    }
}