using System;
using System.Collections.Generic;
using System.Security.Principal;
using OpenRasta.Pipeline;

namespace OpenRasta.Web
{
  public interface ICommunicationContext
  {
    Uri ApplicationBaseUri { get; }

    IRequest Request { get; }
    IResponse Response { get; }

    OperationResult OperationResult { get; set; }

    PipelineData PipelineData { get; }

    IList<Error> ServerErrors { get; }

    IPrincipal User { get; set; }
  }
}