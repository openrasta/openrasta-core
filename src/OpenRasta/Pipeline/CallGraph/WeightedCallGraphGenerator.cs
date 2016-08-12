using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Pipeline.CallGraph
{
  public sealed class WeightedCallGraphGenerator : IGenerateCallGraphs
  {
    public IEnumerable<ContributorCall> GenerateCallGraph(IEnumerable<IPipelineContributor> contributors)
    {
      contributors = contributors.ToList();

      var bootstrapper = contributors.OfType<KnownStages.IBegin>().Single();
      var tree = new DependencyTree<ContributorNotification>(
        new ContributorNotification(bootstrapper, new Notification(Middleware.IdentitySingleTap, contributors)));

      foreach (var contrib in contributors.Where(x => x != bootstrapper))
      {
        var builder = new PipelineBuilder(contributors);

        builder.ContributorRegistrations.Clear();

        contrib.Initialize(builder);

        var contributorRegistrations =
          builder.ContributorRegistrations.DefaultIfEmpty(new Notification(Middleware.IdentitySingleTap, contributors)).ToList();
        foreach (var registration in contributorRegistrations)
        {
          tree.CreateNode(new ContributorNotification(contrib, registration));
        }
      }

      foreach (var notificationNode in tree.Nodes)
      {
        foreach (var parentNode in GetCompatibleTypes(tree,
          notificationNode,
          notificationNode.Value.Notification.AfterTypes))
          parentNode.ChildNodes.Add(notificationNode);
        foreach (var childNode in GetCompatibleTypes(tree,
          notificationNode,
          notificationNode.Value.Notification.BeforeTypes))
          childNode.ParentNodes.Add(notificationNode);
      }

      return
        tree.GetCallGraph()
          .Select(
            x => new ContributorCall(x.Value.Contributor, x.Value.Notification.Target, x.Value.Notification.Description));
    }

    static IEnumerable<DependencyNode<ContributorNotification>> GetCompatibleTypes(
      DependencyTree<ContributorNotification> tree,
      DependencyNode<ContributorNotification> notificationNode,
      IEnumerable<Type> beforeTypes)
    {
      return from childType in beforeTypes
        from compatibleNode in tree.Nodes
        where compatibleNode != notificationNode
              && childType.IsInstanceOfType(compatibleNode.Value.Contributor)
        select compatibleNode;
    }
  }
}
