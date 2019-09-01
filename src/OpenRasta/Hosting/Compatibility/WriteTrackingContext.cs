using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using OpenRasta.CommunicationFeatures;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Compatibility
{
  public class WriteTrackingResponseCommunicationContext : ICommunicationContext
  {
    ICommunicationContext InnerContext { get; }

    public WriteTrackingResponseCommunicationContext(ICommunicationContext context)
    {
      InnerContext = context;
      Response = new WriteTrackingResponse(context.Response);
    }

    public Uri ApplicationBaseUri => InnerContext.ApplicationBaseUri;

    public IRequest Request => InnerContext.Request;
    public IResponse Response { get; }

    public OperationResult OperationResult
    {
      get => InnerContext.OperationResult;
      set => InnerContext.OperationResult = value;
    }

    public PipelineData PipelineData => InnerContext.PipelineData;

    public IList<Error> ServerErrors => InnerContext.ServerErrors;

    public IPrincipal User
    {
      get => InnerContext.User;
      set => InnerContext.User = value;
    }
  }
}