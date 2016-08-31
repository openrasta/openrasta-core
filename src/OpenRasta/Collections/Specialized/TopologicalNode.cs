using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Collections.Specialized
{
    internal sealed class TopologicalNode<T> : IEquatable<TopologicalNode<T>>
    {
        public T Item { get; set; }
        public IList<TopologicalNode<T>> Dependencies { get; private set; }

        public TopologicalNode(T value)
        {
            Item = value;
            Dependencies = new List<TopologicalNode<T>>();
        }

        public bool Equals(TopologicalNode<T> other)
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

            return Equals((TopologicalNode<T>)obj);
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
