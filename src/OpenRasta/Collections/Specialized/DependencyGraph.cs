using System.Collections.Generic;

namespace OpenRasta.Collections.Specialized
{
    public class DependencyGraph<T>
    {
        public IEnumerable<DependencyNode<T>> Nodes { get; private set; }

        public DependencyGraph(T rootItem, IList<DependencyNode<T>> nodes)
        {
            var rootNode = new DependencyNode<T>(rootItem);
            Nodes = TopologicalSort.Sort(rootNode, nodes);
        }
    }
}