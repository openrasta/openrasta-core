using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Collections
{
    // Topological sort based on depth-first algorithm described by Cormen et al. (2001), see http://en.wikipedia.org/wiki/Topological_sorting
    // Credit to Tomas Takac for initial inspiration for this algorithm, see http://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp
    internal class TopologicalSort
    {
        public static IEnumerable<DependencyNodeV2<T>> Sort<T>(DependencyNodeV2<T> rootNode, IList<DependencyNodeV2<T>> nodes)
        {
            var sorted = new List<DependencyNodeV2<T>>();
            var visited = new Dictionary<DependencyNodeV2<T>, bool>();

            Visit(rootNode, sorted, visited);
            nodes.ForEach(n => Visit(n, sorted, visited));

            return sorted;
        }

        private static void Visit<T>(DependencyNodeV2<T> node, IList<DependencyNodeV2<T>> sorted, IDictionary<DependencyNodeV2<T>, bool> visited)
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