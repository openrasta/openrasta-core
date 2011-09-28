JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
using System;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent
{
    public interface IUriDefinition : IRepeatableDefinition<IResourceDefinition>, IHandlerParentDefinition, IUri
    {
        IUriDefinition Named(string uriName);
        IUriDefinition InLanguage(string language);
    }
    public interface IUriDefinition<TResource> : IUriDefinition, IUri<TResource>
    {
        new IResourceDefinition<TResource> And { get; }
    }
}