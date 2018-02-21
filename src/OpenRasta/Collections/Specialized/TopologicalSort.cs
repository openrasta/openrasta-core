using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Collections.Specialized
{
  // Topological sort based on depth-first algorithm described by Cormen et al. (2001), see http://en.wikipedia.org/wiki/Topological_sorting
  // Credit to Tomas Takac for initial inspiration for this algorithm, see http://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp
  internal class TopologicalSort
  {
    public static IEnumerable<TopologicalNode<T>> Sort<T>(IEnumerable<TopologicalNode<T>> nodes)
    {
      var sorted = new List<TopologicalNode<T>>();
      var visited = new Dictionary<TopologicalNode<T>, bool>();

      nodes?.ForEach(n => Visit(n, sorted, visited));

      return sorted;
    }

    static void Visit<T>(TopologicalNode<T> node, IList<TopologicalNode<T>> sorted, IDictionary<TopologicalNode<T>, bool> visited)
    {
      var alreadyVisited = visited.TryGetValue(node, out var inProcess);
      if (alreadyVisited)
      {
        if (inProcess)
          throw new RecursionException("Node contains a circular dependency: " + node);
      }
      else
      {
        visited[node] = true;

        node.DependsOn
            .Where(d => !d.Equals(node))
            .ForEach(d => Visit(d, sorted, visited));

        visited[node] = false;
        sorted.Add(node);
      }
    }
  }
}