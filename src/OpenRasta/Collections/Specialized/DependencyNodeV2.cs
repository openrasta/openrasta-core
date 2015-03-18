using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Collections.Specialized
{
    internal sealed class DependencyNodeV2<T> : IEquatable<DependencyNodeV2<T>>
    {
        public T Item { get; set; }
        public IList<DependencyNodeV2<T>> Dependencies { get; private set; }

        public DependencyNodeV2(T value)
        {
            Item = value;
            Dependencies = new List<DependencyNodeV2<T>>();
        }

        public bool Equals(DependencyNodeV2<T> other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Item.Equals(other.Item);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;

            return Equals((DependencyNodeV2<T>)obj);
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Item: {0}, Dependencies: {1}", Item, string.Join(", ", Dependencies.Select(d => d.Item)));
        }
    }
}