using System.Collections.Generic;

namespace OpenRasta.Collections.Specialized
{
    internal sealed class DependencyGraph<T>
    {
        public IEnumerable<DependencyNodeV2<T>> Nodes { get; private set; }

        public DependencyGraph(T rootItem, IList<DependencyNodeV2<T>> nodes)
        {
            var rootNode = new DependencyNodeV2<T>(rootItem);
            Nodes = TopologicalSort.Sort(rootNode, nodes);
        }
    }
}