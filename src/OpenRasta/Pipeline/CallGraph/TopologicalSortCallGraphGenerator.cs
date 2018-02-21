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


      var rootNodes = nodes.Where(n => !n.DependsOn.Any());
      
      var visitor = new DependedOnVisitor(nodes);

      foreach (var rootNode in rootNodes)
      {
        var earliestNodes =
          (from node in visitor.Visit(rootNode, node => nodesImplementingKnownStages.Contains(node))
            let index = nodesImplementingKnownStages.IndexOf(node)
            where index > 0
            orderby index
            select (node, index)).ToList();

        if (earliestNodes.Any() == false) continue;

        rootNode.DependsOn.Add(nodesImplementingKnownStages[earliestNodes.First().index - 1]);
      }

//      var leafNodes = nodes.Where(dependent => nodes.All(n => n.DependsOn.Contains(dependent) == false));


      return TopologicalSort
        .Sort(nodes)
        .Select(ToContributorCall)
        .ToList();
    }

    static IEnumerable<TopologicalNode<ContributorInvocation>> GetNodesImplementingKnownStages(List<TopologicalNode<ContributorInvocation>> nodes)
    {
      return from knownStageType in _knownStages
        let index = Array.IndexOf(_knownStages, knownStageType)
        let node = nodes.FirstOrDefault(node => knownStageType.IsInstanceOfType(node.Item.Owner))
        where node != null
        orderby index
        select node;
    }

    class DependedOnVisitor
    {
      readonly List<TopologicalNode<ContributorInvocation>> _nodes;
      List<TopologicalNode<ContributorInvocation>> _visited;
      Func<TopologicalNode<ContributorInvocation>, bool> _isSelected;
      List<TopologicalNode<ContributorInvocation>> _selectedNodes;

      public DependedOnVisitor(List<TopologicalNode<ContributorInvocation>> nodes)
      {
        _nodes = nodes;
        _visited = new List<TopologicalNode<ContributorInvocation>>();
      }

      public IEnumerable<TopologicalNode<ContributorInvocation>> Visit(
        TopologicalNode<ContributorInvocation> currentNode,
        Func<TopologicalNode<ContributorInvocation>, bool> isSelected)
      {
        _visited = new List<TopologicalNode<ContributorInvocation>>();
        _selectedNodes = new List<TopologicalNode<ContributorInvocation>>();
        _isSelected = isSelected;

        VisitNode(currentNode);
        return _selectedNodes;
      }

      void VisitNode(TopologicalNode<ContributorInvocation> currentNode)
      {
        if (_visited.Contains(currentNode))
          return;
        foreach (var dependent in _nodes.Where(node => node.DependsOn.Contains(currentNode)))
        {
          _visited.Add(dependent);
          if (_isSelected(dependent))
          {
            _selectedNodes.Add(dependent);
          }
          else
          {
            VisitNode(dependent);
          }
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