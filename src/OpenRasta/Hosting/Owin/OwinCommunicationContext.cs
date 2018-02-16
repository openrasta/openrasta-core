using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using LibOwin;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Katana
{
  class OwinCommunicationContext : ICommunicationContext
  {
    readonly IOwinContext _nativeContext;
    PathString _appBaseRelative;

    public OwinCommunicationContext(IOwinContext nativeContext, ILogger logger)
    {
      PipelineData = new PipelineData(nativeContext.Environment);
      _nativeContext = nativeContext;
      Request = new OwinRequest(nativeContext.Request);
      Response = new OwinResponse(nativeContext);
      ServerErrors = new ServerErrorList { Log = logger };
      _appBaseRelative = nativeContext.Request.PathBase;
    }

    public Uri ApplicationBaseUri => new UriBuilder(
        Request.Uri.Scheme, 
        Request.Uri.Host,
        Request.Uri.Port,
        _nativeContext.Request.PathBase.ToString())
        .Uri;

    public IRequest Request { get; }
    public IResponse Response { get; }
    public OperationResult OperationResult { get; set; }
    public PipelineData PipelineData { get; }
    public IList<Error> ServerErrors { get; }

    public IPrincipal User
    {
      get => _nativeContext.Request.User;
      set => _nativeContext.Request.User = value is ClaimsPrincipal claim ? claim : new ClaimsPrincipal(value);
    }
  }
}