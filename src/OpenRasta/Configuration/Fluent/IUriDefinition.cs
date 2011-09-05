JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
namespace OpenRasta.Configuration.Fluent
{
    public interface IUriDefinition : IRepeatableDefinition<IResourceDefinition>, IHandlerParentDefinition, IUri
    {
        IUriDefinition Named(string uriName);
        IUriDefinition InLanguage(string language);
    }
}