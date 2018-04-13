using System;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Data;
using OpenRasta.Web;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyHandler
  {
    readonly ReverseProxy _proxy;
    readonly ICommunicationContext _context;

    public ReverseProxyHandler(
        ReverseProxy proxy,
        ICommunicationContext context)
    {
      _proxy = proxy;
      _context = context;
    }

    [HttpOperation("*")]
    public async Task<ReverseProxyResponse> Any(Any _)
    {
      if (_context.PipelineData.SelectedResource == null)
        throw new InvalidOperationException(CreateNullResourceLogMessage());

      if (!_context.PipelineData.SelectedResource.ResourceModel.TryGetReverseProxyTarget(out var reverseProxyTarget))
        throw new InvalidOperationException(CreateMissingKeyLogMessage());
      
      return await _proxy.Send(_context, reverseProxyTarget);
    }

    string CreateMissingKeyLogMessage()
    {
      return $"Missing reverse proxy target{Environment.NewLine}" +
             $"{CreateNullResourceLogMessage()}{Environment.NewLine}" +
             $"{_context.PipelineData.SelectedResource.ResourceModel}";
    }

    string CreateNullResourceLogMessage()
    {
      return $"Null resource, invalid condition detected{Environment.NewLine}" +
             $" Uri: {_context.Request.Uri}{Environment.NewLine}" +
             $" Result: {_context.OperationResult}{Environment.NewLine}";
    }

  }
}