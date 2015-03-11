using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Collections.Specialized
{
    public class DependencyNode<T> : IEquatable<DependencyNode<T>>
    {
        public T Item { get; set; }
        public IList<DependencyNode<T>> Dependencies { get; private set; }

        public DependencyNode(T value)
        {
            Item = value;
            Dependencies = new List<DependencyNode<T>>();
        }

        public bool Equals(DependencyNode<T> other)
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

            return Equals((DependencyNode<T>)obj);
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