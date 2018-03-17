using System;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Plugins.Caching.Configuration
{
  public class ResourceMapper<T> : IResourceTarget, IResourceMapper<T>
  {
    readonly IResourceTarget _resource;

    public ResourceMapper(IResource resource)
    {
      _resource = (IResourceTarget) resource;
    }

    public IResourceMapper<T> LastModified(Func<T, DateTimeOffset?> reader)
    {
      _resource.Resource.SetLastModifiedMapper(resource => reader((T) resource));
      return this;
    }

    public IResourceMapper<T> Etag(Func<T, string> reader)
    {
      _resource.Resource.SetEtagMapper(resource => reader((T) resource));
      return this;
    }

    public IResourceMapper<T> Expires(Func<T, TimeSpan> reader)
    {
      _resource.Resource.SetExpires(resource => reader((T) resource));
      return this;
    }

    IMetaModelRepository IFluentTarget.Repository => _resource.Repository;

    ITypeSystem IFluentTarget.TypeSystem => _resource.TypeSystem;

    ResourceModel IResourceTarget.Resource => _resource.Resource;
  }
}