using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Pipeline.CallGraph
{
    public sealed class TopologicalSortCallGraphGenerator : IGenerateCallGraphs
    {
        public IEnumerable<ContributorCall> GenerateCallGraph(PipelineRunner pipelineRunner)
        {
            var bootstrapper = pipelineRunner.Contributors.OfType<KnownStages.IBegin>().Single();
            var nodes = new List<DependencyNodeV2<ContributorNotification>>();

            foreach (var contributor in pipelineRunner.Contributors.Where(x => x != bootstrapper))
            {
                pipelineRunner.NotificationRegistrations.Clear();

                using (pipelineRunner.PipelineLog.Operation(pipelineRunner, "Initializing contributor {0}.".With(contributor.GetType().Name)))
                {
                    contributor.Initialize(pipelineRunner);
                }

                foreach (var reg in pipelineRunner.NotificationRegistrations.DefaultIfEmpty(new Notification(pipelineRunner, null)))
                {
                    nodes.Add(new DependencyNodeV2<ContributorNotification>(new ContributorNotification(contributor, reg)));
                }
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

            var rootItem = new ContributorNotification(bootstrapper, new Notification(pipelineRunner, null));

            return new DependencyGraph<ContributorNotification>(rootItem, nodes).Nodes
                .Select(n => new ContributorCall(n.Item.Contributor, n.Item.Notification.Target, n.Item.Notification.Description));
        }

        static IEnumerable<DependencyNodeV2<ContributorNotification>> GetCompatibleNodes(IEnumerable<DependencyNodeV2<ContributorNotification>> nodes, DependencyNodeV2<ContributorNotification> notificationNode, Type type)
        {
            return from compatibleNode in nodes
                   where !compatibleNode.Equals(notificationNode) && type.IsInstanceOfType(compatibleNode.Item.Contributor)
                   select compatibleNode;
        }
    }
}