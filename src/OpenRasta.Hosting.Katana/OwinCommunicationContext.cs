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
      User = nativeContext.Request.User;
    }

    public Uri ApplicationBaseUri
    {
      get
      {
        var request = _nativeContext.Request;

        var baseUri = "{0}://{1}{2}/".With(request.Uri.Scheme,
          request.Uri.Host,
          request.Uri.IsDefaultPort ? string.Empty : ":" + request.Uri.Port);
        //todo manage the relative path if needed?
        var appBaseUri =
          new Uri(baseUri, UriKind.Absolute); //, new Uri(_host.ApplicationVirtualPath, UriKind.Relative));
        return appBaseUri;
      }
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