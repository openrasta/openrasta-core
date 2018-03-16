using System;
using System.Threading.Tasks;
using OpenRasta.Data;
using OpenRasta.Web;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxiedHandler
  {
    readonly ICommunicationContext context;
    readonly Func<ICommunicationContext, Task<string>> operation;

    public ProxiedHandler(ICommunicationContext context, Func<ICommunicationContext, Task<string>> operation)
    {
      this.context = context;
      this.operation = operation;
    }

    [HttpOperation("*")]
    public async Task<string> Get(Any _)
    {
      return await operation(context);
    }
  }
}