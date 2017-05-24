using System;
using System.Collections.Generic;
using OpenRasta.Collections;

namespace OpenRasta.Web
{
  public abstract class OperationResult
  {
    protected OperationResult()
    {
    }

    protected OperationResult(int httpStatus)
    {
      StatusCode = httpStatus;
      Title = httpStatus + " " + this.GetType().Name;
      Description = ToString();
    }

    public virtual string Description { get; set; }

    public bool IsClientError
    {
      get { return StatusCode >= 400 && StatusCode < 500; }
    }

    public bool IsServerError
    {
      get { return StatusCode >= 500; }
    }

    public Uri RedirectLocation { get; set; }

    /// <summary>
    /// Gets or sets the resource to be returned in the response messsage.
    /// </summary>
    public object ResponseResource { get; set; }

    public virtual string Title { get; set; }
    public virtual int StatusCode { get; set; }
    public virtual int SubStatusCode { get; set; }

    public void Execute(ICommunicationContext context)
    {
      context.Response.StatusCode = StatusCode;
      if (RedirectLocation?.IsAbsoluteUri == true)
      {
        context.Response.Headers["Location"] = RedirectLocation.AbsoluteUri;
      }
      else if (RedirectLocation?.IsAbsoluteUri == false && RedirectLocation.ToString().StartsWith("/"))
      {
        var locationWithoutPrecedingSlash = RedirectLocation.ToString().Substring(1);
        context.Response.Headers["Location"] = new Uri(
          context.ApplicationBaseUri,
          new Uri(locationWithoutPrecedingSlash,UriKind.Relative)).AbsoluteUri;
      }
      else if (RedirectLocation?.IsAbsoluteUri == false && RedirectLocation.ToString().StartsWith("/") == false)
      {
        context.Response.Headers["Location"] = new Uri(context.Request.Uri, RedirectLocation).AbsoluteUri;
      }


      OnExecute(context);
    }

    public override string ToString()
    {
      var responseType = ResponseResource?.GetType().ToString() ?? string.Empty;
      return $"OperationResult: type={GetType().Name}, statusCode={StatusCode} {responseType}";
    }

    protected virtual void OnExecute(ICommunicationContext context)
    {
    }

    public class BadRequest : OperationResult
    {
      public BadRequest() : base(400)
      {
        Errors = new List<Error>();
      }

      public IList<Error> Errors { get; set; }

      protected override void OnExecute(ICommunicationContext context)
      {
        context.Request.Entity.Errors.AddRange(Errors);
        context.Response.Entity.Errors.AddRange(context.Request.Entity.Errors);
        base.OnExecute(context);
      }
    }

    /// <summary>
    /// Represents a "201 Created" response.
    /// </summary>
    public class Created : OperationResult
    {
      public Created() : base(201)
      {
      }

      public Uri CreatedResourceUrl { get; set; }
    }

    public class Forbidden : OperationResult
    {
      public Forbidden() : base(403)
      {
      }
    }

    /// <summary>
    /// Represents a 302 Found response: the requested resource resides temporarily under a different URI.
    /// </summary>
    public class Found : OperationResult
    {
      public Found() : base(302)
      {
      }
    }

    public class PreconditionFailed : OperationResult
    {
      public PreconditionFailed() : base(412)
      {
      }
    }

    public class Gone : OperationResult
    {
      public Gone(ICommunicationContext context) : base(410)
      {
      }
    }

    public class InternalServerError : OperationResult
    {
      public InternalServerError() : base(500)
      {
        //Debugger.Launch();
      }
    }

    public class MethodNotAllowed : OperationResult
    {
      public MethodNotAllowed() : base(405)
      {
      }

      public MethodNotAllowed(Uri requestUri, string methodName, object resourceKey)
        : base(405)
      {
        Title = $"The method {methodName} is not available for this resource.";
        Description =
          $"The requested resource at URI {requestUri} was mapped to a resource with key {resourceKey} and no handler method was available for HTTP method {methodName}";
      }
    }

    public class Modified : OperationResult
    {
      // the lack of svn history makes me wonder why I have this at all... 
      public override int StatusCode
      {
        get { return ResponseResource != null ? 200 : 204; }
        set { }
      }
    }

    public class MovedPermanently : OperationResult
    {
      public MovedPermanently() : base(301)
      {
      }
    }

    public class MovedTemporarily : OperationResult
    {
      public MovedTemporarily() : base(307)
      {
      }
    }

    /// <summary>
    /// Represents a 300 Multiple representations response.
    /// </summary>
    public class MultipleRepresentations : OperationResult
    {
      public MultipleRepresentations() : base(300)
      {
      }
    }

    /// <summary>
    /// Represents a "202 Accepted" response.
    /// </summary>
    public class Accepted : OperationResult
    {
      public Accepted() : base(202)
      {
      }
    }

    /// <summary>
    /// Represents a "204 No content" response.
    /// </summary>
    public class NoContent : OperationResult
    {
      public NoContent() : base(204)
      {
      }
    }

    public class NotFound : OperationResult
    {
      public NotFound() : base(404)
      {
      }

      public override string Title
      {
        get { return "The requested resource was not found."; }
      }


      public NotFoundReason Reason
      {
        get { return (NotFoundReason) SubStatusCode; }
        set { SubStatusCode = (int) value; }
      }
    }

    public class NotModified : OperationResult
    {
      public NotModified() : base(304)
      {
      }
    }

    /// <summary>
    /// Represents a "200 OK" response.
    /// </summary>
    public class OK : OperationResult
    {
      public OK() : this(null)
      {
      }

      public OK(object resource)
        : base(200)
      {
        ResponseResource = resource;
      }
    }

    /// <summary>
    /// The request media type (described by the Content-Type header in the request) could not be understood by the server. This will trigger a 415 error code.
    /// </summary>
    public class RequestMediaTypeUnsupported : OperationResult
    {
      public RequestMediaTypeUnsupported() : base(415)
      {
      }
    }

    /// <summary>
    /// The resource represented by the request could not be returned in any format the client declared supporting through the Accept header.
    /// </summary>
    public class ResponseMediaTypeUnsupported : OperationResult
    {
      public ResponseMediaTypeUnsupported() : base(406)
      {
      }
    }

    /// <summary>
    /// Represents a 303 See other response: the response to the request can be found under a different URI and SHOULD be retrieved using a GET method on that resource.
    /// </summary>
    public class SeeOther : OperationResult
    {
      public SeeOther() : base(303)
      {
      }
    }

    public class Unauthorized : OperationResult
    {
      public Unauthorized() : base(401)
      {
      }
    }
  }
}