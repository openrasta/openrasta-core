using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Collections.Specialized
{
  internal sealed class TopologicalNode<T> : IEquatable<TopologicalNode<T>>
  {
    public T Item { get; }
    public HashSet<TopologicalNode<T>> DependsOn { get; }

    public TopologicalNode(T value)
    {
      Item = value;
      DependsOn = new HashSet<TopologicalNode<T>>();
    }


    public override string ToString()
    {
      return $"Item: {Item}, Dependencies: {string.Join(", ", DependsOn.Select(d => d.Item))}";
    }

    public bool Equals(TopologicalNode<T> other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return EqualityComparer<T>.Default.Equals(Item, other.Item);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      return obj is TopologicalNode<T> node && Equals(node);
    }

    public override int GetHashCode()
    {
      return EqualityComparer<T>.Default.GetHashCode(Item);
    }
  }
}