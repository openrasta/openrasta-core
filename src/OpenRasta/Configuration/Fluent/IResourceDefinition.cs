namespace OpenRasta.Configuration.Fluent
{
    public interface IResourceDefinition : INoIzObject, IResource
    {
        ICodecParentDefinition WithoutUri { get; }
        IUriDefinition AtUri(string uri);
    }
    public interface IResourceDefinition<TResource> : IResourceDefinition,IResource<TResource>
    {
        new ICodecParentDefinition<TResource> WithoutUri { get; }
        new IUriDefinition<TResource> AtUri(string uri);
    }
}