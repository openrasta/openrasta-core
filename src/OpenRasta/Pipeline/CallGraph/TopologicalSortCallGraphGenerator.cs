using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Pipeline.CallGraph
{
  static class TopologicalNodeExtensions
  {
    public static IEnumerable<TopologicalNode<ContributorInvocation>> LeafNodes(
      this List<TopologicalNode<ContributorInvocation>> nodes)
    {
      return nodes.Where(dependent => nodes.All(n => n.DependsOn.Contains(dependent) == false));
    }

    public static IEnumerable<TopologicalNode<ContributorInvocation>> RootNodes(
      this List<TopologicalNode<ContributorInvocation>> nodes)
    {
      return nodes.Where(n => !n.DependsOn.Any());
    }
  }

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
          foreach (var node in GetCompatibleNodes(nodes, currentNode, afterType))
            currentNode.DependsOn.Add(node);
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

      foreach (var rootNode in nodes.RootNodes())
      {
        var earliestNodesAfterRoot =
          (from node in visitor.SelectDescending(node => nodesImplementingKnownStages.Contains(node), rootNode)
            let index = nodesImplementingKnownStages.IndexOf(node)
            orderby index
            select (node, index)).ToList();

        if (earliestNodesAfterRoot.Any() == false) continue;

        var earliestNodeIndex = earliestNodesAfterRoot.First().index;
        if (earliestNodeIndex > 0)
          rootNode.DependsOn.Add(nodesImplementingKnownStages[earliestNodeIndex - 1]);
      }

      foreach (var leaf in nodes.LeafNodes())
      {
        var latestNodesBeforeLeaf =
          (from node in visitor.SelectAscending(node => nodesImplementingKnownStages.Contains(node), leaf)
            let index = nodesImplementingKnownStages.IndexOf(node)
            orderby index descending
            select (node, index)).ToList();

        if (latestNodesBeforeLeaf.Any() == false) continue;

        var latestNodeIndex = latestNodesBeforeLeaf.First().index;
        if (latestNodeIndex < nodesImplementingKnownStages.Count - 1)
          nodesImplementingKnownStages[latestNodeIndex + 1].DependsOn.Add(leaf);
      }


      return visitor.SelectAscending().Select(ToContributorCall).ToList();
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

      public IEnumerable<TopologicalNode<ContributorInvocation>> SelectDescending(
        Func<TopologicalNode<ContributorInvocation>, bool> selector = null,
        TopologicalNode<ContributorInvocation> currentNode = null)
      {
        return Visit(selector, currentNode, node => _nodes.Where(n => n.DependsOn.Contains(node)));
      }

      public IEnumerable<TopologicalNode<ContributorInvocation>> SelectAscending(
        Func<TopologicalNode<ContributorInvocation>, bool> selector = null,
        TopologicalNode<ContributorInvocation> currentNode = null)
      {
        return Visit(selector, currentNode, node => node.DependsOn);
      }

      IEnumerable<TopologicalNode<ContributorInvocation>> Visit(
        Func<TopologicalNode<ContributorInvocation>, bool> selector, TopologicalNode<ContributorInvocation> currentNode,
        Func<TopologicalNode<ContributorInvocation>, IEnumerable<TopologicalNode<ContributorInvocation>>> nextSelector)
      {
        _visited = new Dictionary<TopologicalNode<ContributorInvocation>, bool>();
        _selectedNodes = new List<TopologicalNode<ContributorInvocation>>();
        _isSelected = selector ?? (node => true);

        if (currentNode != null)
          VisitNode(currentNode, nextSelector);
        else
          VisitNodes(_nodes, nextSelector);
        return _selectedNodes;
      }

      void VisitNode(TopologicalNode<ContributorInvocation> currentNode,
        Func<TopologicalNode<ContributorInvocation>, IEnumerable<TopologicalNode<ContributorInvocation>>> nextSelector)
      {
        if (_visited.TryGetValue(currentNode, out bool inProcess))
        {
          if (inProcess) throw new RecursionException($"Node contains a circular dependency: {currentNode}");
          return;
        }

        _visited[currentNode] = true;

        VisitNodes(nextSelector(currentNode).Where(n => !n.Equals(currentNode)).ToList(), nextSelector);

        _visited[currentNode] = false;
        if (_isSelected(currentNode))
          _selectedNodes.Add(currentNode);
      }

      void VisitNodes(List<TopologicalNode<ContributorInvocation>> nextNodes,
        Func<TopologicalNode<ContributorInvocation>, IEnumerable<TopologicalNode<ContributorInvocation>>> nextSelector)
      {
        foreach (var dependent in nextNodes)
        {
          VisitNode(dependent, nextSelector);
        }
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