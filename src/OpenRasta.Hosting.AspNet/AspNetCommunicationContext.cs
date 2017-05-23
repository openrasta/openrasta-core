using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class AspNetCommunicationContext : ICommunicationContext
  {
    const string COMM_CONTEXT_KEY = "__OR_COMM_CONTEXT";

    public static AspNetCommunicationContext Current
    {
      get
      {
        var context = HttpContext.Current;
        if (context.Items.Contains(COMM_CONTEXT_KEY))
          return (AspNetCommunicationContext) context.Items[COMM_CONTEXT_KEY];
        var orContext = new AspNetCommunicationContext(
          TraceSourceLogger.Instance,
          context,
          new AspNetRequest(context),
          new AspNetResponse(context));
        context.Items[COMM_CONTEXT_KEY] = orContext;

        return orContext;
      }
    }

    public AspNetCommunicationContext(ILogger logger, HttpContext context, AspNetRequest request,
      AspNetResponse response)
    {
      NativeContext = context;
      ServerErrors = new ServerErrorList {Log = logger};
      PipelineData = new PipelineData();
      Request = request;
      Response = response;
    }

    public Uri ApplicationBaseUri
    {
      get
      {
        if (NativeContext == null)
          return null;

        string baseUri = "{0}://{1}/".With(NativeContext.Request.Url.Scheme,
          NativeContext.Request.ServerVariables["HTTP_HOST"]);

        var appBaseUri = new Uri(new Uri(baseUri), new Uri(NativeContext.Request.ApplicationPath, UriKind.Relative));
        return appBaseUri;
      }
    }



    public OperationResult OperationResult { get; set; }
    public PipelineData PipelineData { get; set; }
    public IRequest Request { get; private set; }
    public IResponse Response { get; private set; }

    public IList<Error> ServerErrors { get; private set; }

    public IPrincipal User
    {
      get { return NativeContext.User; }
      set { NativeContext.User = value; }
    }

    HttpContext NativeContext { get; set; }
  }
}