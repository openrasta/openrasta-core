using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Configuration;

namespace OpenRasta.Hosting.AspNet
{
  public class ClassicPipeline : AspNetPipeline
  {
    readonly Lazy<IEnumerable<HttpHandlerRegistration>> _handlers =
      new Lazy<IEnumerable<HttpHandlerRegistration>>(ReadHandlers, LazyThreadSafetyMode.PublicationOnly);

    protected override IEnumerable<HttpHandlerRegistration> Handlers => _handlers.Value;


    static IEnumerable<HttpHandlerRegistration> ReadHandlers()
    {
      return ((HttpHandlersSection) WebConfigurationManager.GetSection("system.web/httpHandlers")).Handlers
        .OfType<HttpHandlerAction>()
        .Select(handler => new HttpHandlerRegistration(handler.Verb, handler.Path, handler.Type))
        .Where(IsHandlerRegistrationValid)
        .ToList();
    }
  }
}