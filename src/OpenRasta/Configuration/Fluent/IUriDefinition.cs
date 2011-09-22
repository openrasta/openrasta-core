namespace OpenRasta.Configuration.Fluent
{
    public interface IUriDefinition : 
        IHandlerParentDefinition, 
        IUri
    {
        IUriDefinition Named(string uriName);
        IUriDefinition InLanguage(string language);
        IResourceDefinition And { get; }
    }
    public interface IUriDefinition<TResource> : 
        IUriDefinition, 
        IHandlerParentDefinition<TResource> ,
        IUri<TResource>
    {
        new IResourceDefinition<TResource> And { get; }
    }
}