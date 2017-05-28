using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.TypeSystem.Surrogates;

namespace OpenRasta.TypeSystem.Surrogated
{
    public class SurrogateBuilderProvider : ISurrogateProvider
    {
        readonly IEnumerable<ISurrogateBuilder> _builders;
        static readonly ConcurrentDictionary<IType, IType> _typeCache = new ConcurrentDictionary<IType, IType>();
        static readonly ConcurrentDictionary<IProperty, IProperty> _propCache = new ConcurrentDictionary<IProperty, IProperty>();
 
        public SurrogateBuilderProvider(IEnumerable<ISurrogateBuilder> builders)
        {
          _builders = builders ?? throw new ArgumentNullException(nameof(builders));
        }

        public T FindSurrogate<T>(T member) where T : IMember
        {
            var t = member as IType;
            if (t != null) return (T)FindTypeSurrogate(t);
            var p = member as IProperty;
            if (p != null) return (T)FindPropertySurrogate(p);

            return member;
        }

        IProperty FindPropertySurrogate(IProperty property)
        {
            return Cached(_propCache,
                          property,
                          p =>
                          {
                              var alienTypes = _builders.Where(x => x.CanCreateFor(p)).Select(x => x.Create(p)).ToList();
                              return alienTypes.Count > 0 ? new PropertyWithSurrogates(property, alienTypes) : property;
                          });
        }

        IType FindTypeSurrogate(IType type)
        {
            return Cached(_typeCache,
                          type,
                          t =>
                          {
                              var surrogates = _builders.Where(x => x.CanCreateFor(t)).Select(x => x.Create(t)).ToList();
                              return surrogates.Count > 0 ? new TypeWithSurrogates(type, surrogates) : type;
                          });

        }
        T Cached<T>(ConcurrentDictionary<T, T> cache, T value, Func<T, T> createCached)
        {
            return cache.GetOrAdd(value, createCached);
        }
    }
}