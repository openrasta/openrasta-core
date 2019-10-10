using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections.Specialized;
using OpenRasta.Pipeline.Contributors;

namespace OpenRasta.Pipeline.CallGraph
{
  public sealed class WeightedCallGraphGenerator : IGenerateCallGraphs
  {
    public IEnumerable<ContributorCall> GenerateCallGraph(IEnumerable<IPipelineContributor> contributors)
    {
      var contribList = contributors.ToList();

      var bootstrapper = contribList.OfType<KnownStages.IBegin>().SingleOrDefault();
      bool isSyntheticBootstrap = false;
      if (bootstrapper == null)
      {
        bootstrapper = new PreExecutingContributor();
        contribList.Add(bootstrapper);
        isSyntheticBootstrap = true;
      }

      contributors = contribList;

      var tree = new DependencyTree<ContributorInvocation>();

      foreach (var node in new ContributorBuilder().Build(contributors))
        tree.CreateNode(node);
      
      var rootNode = tree.Nodes.First(n => n.Value.Owner is KnownStages.IBegin);
      
      foreach (var notificationNode in tree.Nodes)
      {
        foreach (var parentNode in GetCompatibleTypes(tree,
            notificationNode,
            notificationNode.Value.AfterTypes))
          parentNode.ChildNodes.Add(notificationNode);
        foreach (var childNode in GetCompatibleTypes(tree,
            notificationNode,
            notificationNode.Value.BeforeTypes))
          childNode.ParentNodes.Add(notificationNode);
      }

      return tree
          .GetCallGraph(rootNode)
          .Where(n => !isSyntheticBootstrap || n.Value.Owner != bootstrapper)
          .Select(x => new ContributorCall(
              x.Value.Owner,
              x.Value.Target,
              x.Value.Description));
    }

    static IEnumerable<DependencyNode<ContributorInvocation>> GetCompatibleTypes(
        DependencyTree<ContributorInvocation> tree,
        DependencyNode<ContributorInvocation> notificationNode,
        IEnumerable<Type> beforeTypes)
    {
      return from childType in beforeTypes
          from compatibleNode in tree.Nodes
          where compatibleNode != notificationNode
                && childType.IsInstanceOfType(compatibleNode.Value.Owner)
          select compatibleNode;
    }
  }
}