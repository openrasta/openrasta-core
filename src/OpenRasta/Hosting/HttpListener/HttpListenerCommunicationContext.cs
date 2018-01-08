using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.HttpListener
{
  public class HttpListenerCommunicationContext : ICommunicationContext
  {
    readonly IHost _host;
    readonly HttpListenerContext _nativeContext;

    public HttpListenerCommunicationContext(IHost host, HttpListenerContext nativeContext, ILogger logger)
    {
      ServerErrors = new ServerErrorList {Log = logger};
      PipelineData = new PipelineData
      {
        Owin =
        {
          SslLoadClientCertAsync = LoadClientCert
        }
      };
      _host = host;
      _nativeContext = nativeContext;
      User = nativeContext.User;
      
      Request = new HttpListenerRequest(this, nativeContext.Request);
      Response = new HttpListenerResponse(this, nativeContext.Response);
    }

    async Task LoadClientCert()
    {
      PipelineData.Owin.SslClientCertificate = await _nativeContext.Request.GetClientCertificateAsync();
    }

    public Uri ApplicationBaseUri
    {
      get
      {
        var request = _nativeContext.Request;

        var baseUri =
          $"{request.Url.Scheme}://{request.Url.Host}{(request.Url.IsDefaultPort ? string.Empty : ":" + request.Url.Port)}/";

        var appBaseUri = new Uri(new Uri(baseUri, UriKind.Absolute),
          new Uri(_host.ApplicationVirtualPath, UriKind.Relative));
        return appBaseUri;
      }
    }

    public OperationResult OperationResult { get; set; }

    public PipelineData PipelineData { get; }

    public IRequest Request { get; }

    public IResponse Response { get; }

    public IList<Error> ServerErrors { get; }

    public IPrincipal User { get; set; }
  }
}