using System;
using System.Linq.Expressions;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Configuration.Fluent
{
  public interface IResourceDefinition : INoIzObject, IResource
  {
    ResourceModel Resource { get; }
    ICodecParentDefinition WithoutUri { get; }
    IUriDefinition AtUri(string uri);
  }

  public interface IResourceDefinition<T> : IResourceDefinition, IResource<T>
  {
    new IUriDefinition<T> AtUri(string uri);

    IUriDefinition<T> AtUri(Expression<Func<T, string>> uri);
  }

  public static class ResourceNamingExtensions
  {
    public static IResourceDefinition<T> Named<T>(this IResourceDefinition<T> resource, string name)
    {
      resource.Resource.ResourceKey = Tuple.Create(name,typeof(T));
      resource.Resource.ResourceType = typeof(T);
      resource.Resource.Name = name;
      return resource;
    }
  }
}