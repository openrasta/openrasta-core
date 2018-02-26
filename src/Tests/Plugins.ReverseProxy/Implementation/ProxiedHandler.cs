using System;
using OpenRasta.Data;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxiedHandler
  {
    readonly ICommunicationContext context;
    readonly Func<ICommunicationContext, string> operation;

    public ProxiedHandler(ICommunicationContext context, Func<ICommunicationContext, string> operation)
    {
      this.context = context;
      this.operation = operation;
    }

    [HttpOperation("*")]
    public string Get(Any _)
    {
      return operation(context);
    }
  }
}