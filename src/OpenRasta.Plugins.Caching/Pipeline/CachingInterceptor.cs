using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Pipeline;
using OpenRasta.Plugins.Caching.Providers;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public class CachingInterceptor : IOperationInterceptorAsync
  {
    readonly PipelineData _data;
    CacheProxyAttribute _proxy;
    CacheClientAttribute _client;

    public CachingInterceptor(ICommunicationContext context)
    {
      _data = context.PipelineData;
    }

    public Func<IOperationAsync, Task<IEnumerable<OutputMember>>> Compose(
      Func<IOperationAsync, Task<IEnumerable<OutputMember>>> next)
    {
      return async operation =>
      {
        var outputMembers = await next(operation);

        _proxy = operation.FindAttribute<CacheProxyAttribute>();
        _client = operation.FindAttribute<CacheClientAttribute>();

        _data[Keys.RESPONSE_CACHE] = CacheResponse.GetResponseDirective(_proxy, _client);
        return outputMembers;
      };
    }
  }
}