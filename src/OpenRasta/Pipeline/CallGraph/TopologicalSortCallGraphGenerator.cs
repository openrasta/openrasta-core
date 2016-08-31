using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OpenRasta.Collections;
using OpenRasta.Collections.Specialized;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.CallGraph
{
  public sealed class TopologicalSortCallGraphGenerator : IGenerateCallGraphs
  {
      static TopologicalSortCallGraphGenerator()
      {
      }

      public IEnumerable<ContributorCall> GenerateCallGraph(IEnumerable<IPipelineContributor> contributors)
    {
      contributors = contributors.ToList();

      var bootstrapper = contributors.OfType<KnownStages.IBegin>().Single();
      var nodes = new List<TopologicalNode<ContributorNotification>>();

      foreach (var contributor in contributors.Where(x => x != bootstrapper))
      {
        var builder = new PipelineBuilder(contributors);
        builder.ContributorRegistrations.Clear();

        contributor.Initialize(builder);

        nodes.AddRange(
          builder.ContributorRegistrations
              .DefaultIfEmpty(new Notification(
                  Middleware.IdentitySingleTap,
                  builder.Contributors))
              .Select(reg => new TopologicalNode<ContributorNotification>(
                  new ContributorNotification(contributor, reg))));
      }

      foreach (var notificationNode in nodes)
      {
        foreach (var afterType in notificationNode.Item.Notification.AfterTypes)
        {
          var parents = GetCompatibleNodes(nodes, notificationNode, afterType);
          notificationNode.Dependencies.AddRange(parents);
        }

        foreach (var beforeType in notificationNode.Item.Notification.BeforeTypes)
        {
          var children = GetCompatibleNodes(nodes, notificationNode, beforeType);
          foreach (var child in children)
          {
            child.Dependencies.Add(notificationNode);
          }
        }
      }

      var rootItem = new ContributorNotification(bootstrapper,
          new Notification(Middleware.IdentitySingleTap, contributors));

      return new TopologicalTree<ContributorNotification>(rootItem, nodes).Nodes
        .Select(
          n => new ContributorCall(n.Item.Contributor, n.Item.Notification.Target, n.Item.Notification.Description));
    }

    static IEnumerable<TopologicalNode<ContributorNotification>> GetCompatibleNodes(
      IEnumerable<TopologicalNode<ContributorNotification>> nodes,
      TopologicalNode<ContributorNotification> notificationNode, Type type)
    {
      return from compatibleNode in nodes
        where !compatibleNode.Equals(notificationNode) && type.IsInstanceOfType(compatibleNode.Item.Contributor)
        select compatibleNode;
    }
  }
}
