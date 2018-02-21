using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Pipeline.CallGraph
{
  public sealed class TopologicalSortCallGraphGenerator : IGenerateCallGraphs
  {
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

      var rootNodes = nodes.Where(n => !n.DependsOn.Any());
      var leafNodes = nodes.Where(dependent => nodes.All(n => n.DependsOn.Contains(dependent) == false));
      
     
      var syntheticRootNode = new TopologicalNode<ContributorInvocation>(
          new ContributorInvocation(new SyntheticFirstContributor(), Middleware.IdentitySingleTap));

//      foreach (var node in rootNodes)
//        node.DependsOn.Add(syntheticRootNode);
//
//      var syntheticEndNode = new TopologicalNode<ContributorInvocation>(
//          new ContributorInvocation(new SyntheticLastContributor(), Middleware.IdentitySingleTap));
//
//      foreach (var leafNode in leafNodes)
//        syntheticEndNode.DependsOn.Add(leafNode);

      return TopologicalSort
          .Sort(nodes)
          .Except(new[]{syntheticRootNode})
          .Select(ToContributorCall)
          .ToList();
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