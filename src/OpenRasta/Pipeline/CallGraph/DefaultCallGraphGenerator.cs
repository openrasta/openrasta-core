using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections.Specialized;

namespace OpenRasta.Pipeline.CallGraph
{
    public sealed class DefaultCallGraphGenerator : IGenerateCallGraphs
    {
        public IEnumerable<ContributorCall> GenerateCallGraph(PipelineRunner pipelineRunner)
        {
            var bootstrapper = pipelineRunner.Contributors.OfType<KnownStages.IBegin>().Single();
            var tree = new DependencyTree<ContributorNotification>(
                new ContributorNotification(bootstrapper, new Notification(pipelineRunner, null)));

            foreach (var contrib in pipelineRunner.Contributors.Where(x => x != bootstrapper))
            {
                pipelineRunner.NotificationRegistrations.Clear();
                using (pipelineRunner.PipelineLog.Operation(pipelineRunner, "Initializing contributor {0}.".With(contrib.GetType().Name)))
                    contrib.Initialize(pipelineRunner);

                foreach (var reg in pipelineRunner.NotificationRegistrations.DefaultIfEmpty(new Notification(pipelineRunner, null)))
                {
                    tree.CreateNode(new ContributorNotification(contrib, reg));
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

            return tree.GetCallGraph().Select(x => new ContributorCall(x.Value.Contributor, x.Value.Notification.Target, x.Value.Notification.Description));
        }

        static IEnumerable<DependencyNode<ContributorNotification>> GetCompatibleTypes(DependencyTree<ContributorNotification> tree,
                                                                                DependencyNode<ContributorNotification> notificationNode,
                                                                                IEnumerable<Type> beforeTypes)
        {
            return from childType in beforeTypes
                   from compatibleNode in tree.Nodes
                   where compatibleNode != notificationNode
                         && childType.IsAssignableFrom(compatibleNode.Value.Contributor.GetType())
                   select compatibleNode;
        }
    }
}
