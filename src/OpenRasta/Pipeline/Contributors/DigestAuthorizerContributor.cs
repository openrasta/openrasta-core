// Digest Authentication implementation
//  Inspired by mono's implenetation, rewritten for OpenRasta.
// Original authors:
//  Greg Reinacker (gregr@rassoc.com)
//  Sebastien Pouliot (spouliot@motus.com)
// Portions (C) 2002-2003 Greg Reinacker, Reinacker & Associates, Inc. All rights reserved.
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Original source code available at
// http://www.rassoc.com/gregr/weblog/stories/2002/07/09/webServicesSecurityHttpDigestAuthenticationWithoutActiveDirectory.html

using System;
using System.Linq;
using System.Security.Principal;
using OpenRasta.Security;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class DigestAuthorizerContributor : IPipelineContributor
  {
    // injected property, optional by default
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public IAuthenticationProvider AuthProvider { get; set; }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ReadCredentials)
        .After<KnownStages.IBegin>()
        .And
        .Before<KnownStages.IHandlerSelection>();

      pipelineRunner.Notify(WriteCredentialRequest)
        .After<KnownStages.IOperationResultInvocation>()
        .And
        .Before<KnownStages.IResponseCoding>();
    }

    public PipelineContinuation ReadCredentials(ICommunicationContext context)
    {
      if (AuthProvider == null)
        return PipelineContinuation.Continue;

      var authorizeHeader = GetDigestHeader(context);

      if (authorizeHeader == null)
        return PipelineContinuation.Continue;

      var digestUri = GetAbsolutePath(authorizeHeader.Uri);

      if (digestUri != context.Request.Uri.AbsolutePath)
        return ClientError(context);

      var creds = AuthProvider.GetByUsername(authorizeHeader.Username);

      if (creds == null)
        return NotAuthorized(context);
      var checkHeader = new DigestHeader(authorizeHeader)
      {
        Password = creds.Password,
        Uri = authorizeHeader.Uri
      };
      var hashedDigest = checkHeader.GetCalculatedResponse(context.Request.HttpMethod);

      if (authorizeHeader.Response != hashedDigest) return NotAuthorized(context);
      
      IIdentity id = new GenericIdentity(creds.Username, "Digest");
      context.User = new GenericPrincipal(id, creds.Roles);
      return PipelineContinuation.Continue;
    }

    static DigestHeader GetDigestHeader(ICommunicationContext context)
    {
      var header = context.Request.Headers["Authorization"];
      return string.IsNullOrEmpty(header) ? null : DigestHeader.Parse(header);
    }

    static PipelineContinuation ClientError(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.BadRequest();
      return PipelineContinuation.RenderNow;
    }

    static string GetAbsolutePath(string uri)
    {
      uri = uri.TrimStart();

      if (uri.StartsWith("http://") || uri.StartsWith("https://"))
      {
        return new Uri(uri).AbsolutePath;
      }
      return uri.Any(ch => ch > 127) ? Uri.EscapeUriString(uri) : uri;
    }

    static PipelineContinuation NotAuthorized(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.Unauthorized();
      return PipelineContinuation.RenderNow;
    }

    static PipelineContinuation WriteCredentialRequest(ICommunicationContext context)
    {
      if (context.OperationResult is OperationResult.Unauthorized)
      {
        context.Response.Headers["WWW-Authenticate"] =
          new DigestHeader
            {
              Realm = "Digest Authentication",
              QualityOfProtection = "auth",
              Nonce = "nonce",
              Stale = false,
              Opaque = "opaque"
            }
            .ServerResponseHeader;
      }
      return PipelineContinuation.Continue;
    }
  }
}