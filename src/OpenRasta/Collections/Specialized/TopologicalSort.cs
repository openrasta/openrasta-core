using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Collections.Specialized
{
    // Topological sort based on depth-first algorithm described by Cormen et al. (2001), see http://en.wikipedia.org/wiki/Topological_sorting
    // Credit to Tomas Takac for initial inspiration for this algorithm, see http://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp
    internal class TopologicalSort
    {
        public static IEnumerable<TopologicalNode<T>> Sort<T>(TopologicalNode<T> rootNode, IList<TopologicalNode<T>> nodes)
        {
            var sorted = new List<TopologicalNode<T>>();
            var visited = new Dictionary<TopologicalNode<T>, bool>();

            Visit(rootNode, sorted, visited);
            nodes.ForEach(n => Visit(n, sorted, visited));

            return sorted;
        }

        private static void Visit<T>(TopologicalNode<T> node, IList<TopologicalNode<T>> sorted, IDictionary<TopologicalNode<T>, bool> visited)
        {
            bool inProcess;
            var alreadyVisited = visited.TryGetValue(node, out inProcess);
            if (alreadyVisited)
            {
                if (inProcess)
                    throw new RecursionException("Node contains a circular dependency: " + node);
            }
            else
            {
                visited[node] = true;

                node.Dependencies.Where(d => !d.Equals(node))
                    .ForEach(d => Visit(d, sorted, visited));

                visited[node] = false;
                sorted.Add(node);
            }
        }
    }
}
