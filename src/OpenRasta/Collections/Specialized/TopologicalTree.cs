using System.Collections.Generic;

namespace OpenRasta.Collections.Specialized
{
    internal sealed class TopologicalTree<T>
    {
        public IEnumerable<TopologicalNode<T>> Nodes { get; private set; }

        public TopologicalTree(T rootItem, IList<TopologicalNode<T>> nodes)
        {
            var rootNode = new TopologicalNode<T>(rootItem);
            Nodes = TopologicalSort.Sort(rootNode, nodes);
        }
    }
}
