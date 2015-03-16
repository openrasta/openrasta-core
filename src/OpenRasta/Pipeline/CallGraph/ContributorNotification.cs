namespace OpenRasta.Pipeline.CallGraph
{
    internal struct ContributorNotification
    {
        public readonly IPipelineContributor Contributor;
        public readonly Notification Notification;

        public ContributorNotification(IPipelineContributor contributor, Notification notification)
        {
            Notification = notification;
            Contributor = contributor;
        }

        public override string ToString()
        {
            return Contributor.ToString();
        }
    }
}