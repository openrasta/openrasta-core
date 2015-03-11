using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Collections
{
    // Topological sort based on depth-first algorithm described by Cormen et al. (2001), see http://en.wikipedia.org/wiki/Topological_sorting
    // Credit to Tomas Takac for initial inspiration for this algorithm, see http://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp
    internal class TopologicalSort
    {
        public static IEnumerable<DependencyNode<T>> Sort<T>(DependencyNode<T> rootNode, IList<DependencyNode<T>> nodes)
        {
            var sorted = new List<DependencyNode<T>>();
            var visited = new Dictionary<DependencyNode<T>, bool>();

            Visit(rootNode, sorted, visited);
            nodes.ForEach(n => Visit(n, sorted, visited));

            return sorted;
        }

        private static void Visit<T>(DependencyNode<T> node, IList<DependencyNode<T>> sorted, IDictionary<DependencyNode<T>, bool> visited)
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