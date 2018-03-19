using System;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Plugins.Caching.Configuration;
using OpenRasta.Plugins.Caching.Pipeline;
using OpenRasta.Web;

// ReSharper disable once CheckNamespace
namespace OpenRasta.Configuration
{
  public static class CachingConfigurationExtensions
  {
    public static T Caching<T>(this T uses) where T : IUses
    {
      uses.PipelineContributor(() => new ConditionalEtagContributor());
      uses.PipelineContributor(() => new ConditionalLastModifiedContributor());
      uses.PipelineContributor(() => new ConditionalEtagContributor());
      uses.PipelineContributor(() => new CacheDirectivesContributor());

      uses.PipelineContributor((IMetaModelRepository repository) => new EntityEtagContributor(repository));
      uses.PipelineContributor((IMetaModelRepository repository) => new EntityLastModified(repository));

      uses.Dependency(ctx => ctx
        .Transient((ICommunicationContext context) => new CachingInterceptor(context))
        .As<IOperationInterceptorAsync>());
      
      return uses;
    }
    
    public static IResourceDefinition<T> LastModified<T>(
      this IResourceDefinition<T> resource, 
     Func<T, DateTimeOffset?> reader)
    {
      ((IResourceTarget)resource).Resource.SetLastModifiedMapper(r => reader((T) r));
      return resource;
    }

    public static IResourceDefinition<T> Etag<T>(this IResourceDefinition<T> resource, Func<T, string> reader)
    {
      ((IResourceTarget)resource).Resource.SetEtagMapper(r => reader((T) r));
      return resource;
    }

    public static IResourceDefinition<T> Expires<T>(this IResourceDefinition<T> resource, Func<T, TimeSpan> reader)
    {
      ((IResourceTarget)resource).Resource.SetExpires(r => reader((T) r));
      return resource;
    }

  }
}