using System;
using System.Linq.Expressions;

namespace OpenRasta.Configuration.Fluent
{
  public interface IResourceDefinition : INoIzObject, IResource
  {
    ICodecParentDefinition WithoutUri { get; }
    IUriDefinition AtUri(string uri);
  }

  public interface IResourceDefinition<T> : IResourceDefinition, IResource<T>
  {
    new IUriDefinition<T> AtUri(string uri);

    IUriDefinition<T> AtUri(Expression<Func<T, string>> uri);
  }
}