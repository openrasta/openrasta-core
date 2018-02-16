﻿using System;
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
    public string Get(AnyParameters _)
    {
      return operation(context);
    }
  }
}