using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyHandler
  {
    readonly ReverseProxy _proxy;
    readonly IMetaModelRepository _metamodelRepository;
    readonly ICommunicationContext _context;

    public ReverseProxyHandler(
        ReverseProxy proxy,
        IMetaModelRepository metamodelRepository,
        ICommunicationContext context)
    {
      _proxy = proxy;
      _metamodelRepository = metamodelRepository;
      _context = context;
    }

    [HttpOperation("*")]
    public async Task<HttpResponseMessage> Any(AnyParameters _)
    {
      return await _proxy.Send(_context, CurrentResourceModel.GetReverseProxyTarget());
    }

    ResourceModel CurrentResourceModel => _metamodelRepository
        .ResourceRegistrations
        .Single(reg => reg.ResourceKey == _context.PipelineData.SelectedResource.ResourceKey);
  }
}