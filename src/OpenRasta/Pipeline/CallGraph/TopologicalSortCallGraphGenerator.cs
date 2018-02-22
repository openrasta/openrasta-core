using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Pipeline.CallGraph
{
  public sealed class TopologicalSortCallGraphGenerator : IGenerateCallGraphs
  {
    static readonly Type[] _knownStages =
    {
      typeof(KnownStages.IBegin),
      typeof(KnownStages.IAuthentication),
      typeof(KnownStages.IUriMatching),
      typeof(KnownStages.IHandlerSelection),
      typeof(KnownStages.IOperationCreation),
      typeof(KnownStages.IOperationFiltering),
      typeof(KnownStages.ICodecRequestSelection),
      typeof(KnownStages.IRequestDecoding),
      typeof(KnownStages.IOperationExecution),
      typeof(KnownStages.IOperationResultInvocation),
      typeof(KnownStages.ICodecResponseSelection),
      typeof(KnownStages.IResponseCoding),
      typeof(KnownStages.IEnd)
    };

    public IEnumerable<ContributorCall> GenerateCallGraph(IEnumerable<IPipelineContributor> contributors)
    {
      var nodes = new ContributorBuilder()
        .Build(contributors)
        .Select(c => new TopologicalNode<ContributorInvocation>(c))
        .ToList();


      foreach (var currentNode in nodes)
      {
        foreach (var afterType in currentNode.Item.AfterTypes)
        {
          currentNode.DependsOn.AddRange(GetCompatibleNodes(nodes, currentNode, afterType));
        }

        foreach (var beforeType in currentNode.Item.BeforeTypes)
        {
          foreach (var child in GetCompatibleNodes(nodes, currentNode, beforeType))
          {
            child.DependsOn.Add(currentNode);
          }
        }
      }

      var nodesImplementingKnownStages = GetNodesImplementingKnownStages(nodes).ToList();


      var visitor = new TopologicalTreeVisitor(nodes);

      foreach (var rootNode in nodes.Where(n => !n.DependsOn.Any()))
      {
        var earliestNodesAfterRoot =
          (from node in visitor.VisitDescending(rootNode, node => nodesImplementingKnownStages.Contains(node))
            let index = nodesImplementingKnownStages.IndexOf(node)
            where index > 0
            orderby index
            select (node, index)).ToList();

        if (earliestNodesAfterRoot.Any() == false) continue;
        rootNode.DependsOn.Add(nodesImplementingKnownStages[earliestNodesAfterRoot.First().index - 1]);
      }

      var leafNodes = nodes.Where(dependent => nodes.All(n => n.DependsOn.Contains(dependent) == false));
      foreach (var leaf in leafNodes)
      {
        var latestNodesBeforeLeaf =
          (from node in visitor.VisitUp(leaf, node => nodesImplementingKnownStages.Contains(node))
            let index = nodesImplementingKnownStages.IndexOf(node)
            where index >= 0 && index < nodesImplementingKnownStages.Count -1
            orderby index descending
            select (node, index)).ToList();
        
        if (latestNodesBeforeLeaf.Any() == false) continue;
        var latestNode = latestNodesBeforeLeaf.First();
        nodesImplementingKnownStages[latestNodesBeforeLeaf.First().index + 1].DependsOn.Add(leaf);
      }


      return TopologicalSort
        .Sort(nodes)
        .Select(ToContributorCall)
        .ToList();
    }

    static IEnumerable<TopologicalNode<ContributorInvocation>> GetNodesImplementingKnownStages(
      List<TopologicalNode<ContributorInvocation>> nodes)
    {
      return from knownStageType in _knownStages
        let index = Array.IndexOf(_knownStages, knownStageType)
        let node = nodes.FirstOrDefault(node => knownStageType.IsInstanceOfType(node.Item.Owner))
        where node != null
        orderby index
        select node;
    }

    class TopologicalTreeVisitor
    {
      readonly List<TopologicalNode<ContributorInvocation>> _nodes;
      Dictionary<TopologicalNode<ContributorInvocation>, bool> _visited;
      Func<TopologicalNode<ContributorInvocation>, bool> _isSelected;
      List<TopologicalNode<ContributorInvocation>> _selectedNodes;

      public TopologicalTreeVisitor(List<TopologicalNode<ContributorInvocation>> nodes)
      {
        _nodes = nodes;
      }

      public IEnumerable<TopologicalNode<ContributorInvocation>> VisitDescending(
        TopologicalNode<ContributorInvocation> currentNode,
        Func<TopologicalNode<ContributorInvocation>, bool> isSelected)
      {
        _visited = new Dictionary<TopologicalNode<ContributorInvocation>, bool>();
        _selectedNodes = new List<TopologicalNode<ContributorInvocation>>();
        _isSelected = isSelected;

        VisitNode(currentNode, node=>_nodes.Where(n => n.DependsOn.Contains(node)));
        return _selectedNodes;
      }

      public IEnumerable<TopologicalNode<ContributorInvocation>> VisitUp(
        TopologicalNode<ContributorInvocation> currentNode,
        Func<TopologicalNode<ContributorInvocation>, bool> isSelected)
      {
        _visited = new Dictionary<TopologicalNode<ContributorInvocation>, bool>();
        _selectedNodes = new List<TopologicalNode<ContributorInvocation>>();
        _isSelected = isSelected;

        VisitNode(currentNode, node=>node.DependsOn);
        return _selectedNodes;
      }
      void VisitNode(TopologicalNode<ContributorInvocation> currentNode, 
        Func<TopologicalNode<ContributorInvocation>, IEnumerable<TopologicalNode<ContributorInvocation>>> nextSelector)
      {
        if (_visited.TryGetValue(currentNode, out bool inProcess))
        {
          if (inProcess) throw new RecursionException();
          return;
        }

        _visited[currentNode] = true;
        foreach (var dependent in nextSelector(currentNode))
        {
          if (_isSelected(dependent))
            _selectedNodes.Add(dependent);
          else
            VisitNode(dependent, nextSelector);
        }

        _visited[currentNode] = false;
      }
    }

    static ContributorCall ToContributorCall(TopologicalNode<ContributorInvocation> node)
    {
      return new ContributorCall(node.Item.Owner, node.Item.Target, node.Item.Description);
    }


    static IEnumerable<TopologicalNode<ContributorInvocation>> GetCompatibleNodes(
      List<TopologicalNode<ContributorInvocation>> nodes,
      TopologicalNode<ContributorInvocation> notificationNode,
      Type type)
    {
      return from compatibleNode in nodes
        where !compatibleNode.Equals(notificationNode) && type.IsInstanceOfType(compatibleNode.Item.Owner)
        select compatibleNode;
    }
  }
}