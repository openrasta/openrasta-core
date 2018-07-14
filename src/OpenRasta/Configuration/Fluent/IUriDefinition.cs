using System;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent
{
  public interface IUriDefinition :
    IRepeatableDefinition<IResourceDefinition>, IHandlerParentDefinition, IUri
  {
    UriModel Uri { get; }
    IUriDefinition Named(string uriName);
    IUriDefinition InLanguage(string language);
  }

  public interface IUriDefinition<TResource> : IUriDefinition, IUri<TResource>
  {
    new IResourceDefinition<TResource> And { get; }
  }
}