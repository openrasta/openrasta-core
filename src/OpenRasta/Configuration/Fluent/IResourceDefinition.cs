JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
namespace OpenRasta.Configuration.Fluent
{
    public interface IResourceDefinition : INoIzObject, IResource
    {
        ICodecParentDefinition WithoutUri { get; }
        IUriDefinition AtUri(string uri);
    }
}