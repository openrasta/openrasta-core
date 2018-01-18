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

    public OwinCommunicationContext(IOwinContext nativeContext, ILogger logger)
    {
      PipelineData = new PipelineData(nativeContext.Environment);
      _nativeContext = nativeContext;
      Request = new OwinRequest(nativeContext.Request);
      Response = new OwinResponse(nativeContext);
      ServerErrors = new ServerErrorList {Log = logger};
      ApplicationBaseUri = ComputeApplicationBaseUri();
    }

    public Uri ApplicationBaseUri { get; }

    Uri ComputeApplicationBaseUri()
    {
      var request = _nativeContext.Request;
      var uriBuilder = new UriBuilder(request.Uri.Scheme, request.Uri.Host, request.Uri.Port, request.PathBase.ToString());

      return uriBuilder.Uri;
    }

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