#region License

/* Authors:
 *      Dylan Beattie (dylan@dylanbeattie.net)
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2015 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */

#endregion

// HTTP Basic Authentication implementation 
// Adapts approach used in https://github.com/scottlittlewood/OpenRastaAuthSample to support pipeline contributor model.

using System;
using System.Security.Principal;
using OpenRasta.Security;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class BasicAuthorizerContributor : IPipelineContributor
  {
    
    // ReSharper disable once MemberCanBePrivate.Global -- injected
    public IAuthenticationProvider AuthProvider { get; set; }


    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ReadCredentials)
        .After<KnownStages.IBegin>()
        .And
        .Before<KnownStages.IHandlerSelection>();
    }

    PipelineContinuation ReadCredentials(ICommunicationContext context)
    {
      if (AuthProvider == null)
        return PipelineContinuation.Continue;

      var header = ReadBasicAuthHeader(context);
      if (header == null)
        return PipelineContinuation.Continue;

      var credentials = AuthProvider.GetByUsername(header.Username);

      if (!AuthProvider.ValidatePassword(credentials, header.Password))
        return PipelineContinuation.Continue;
      IIdentity id = new GenericIdentity(credentials.Username, "Basic");
      context.User = new GenericPrincipal(id, credentials.Roles);

      return PipelineContinuation.Continue;
    }

    static BasicAuthorizationHeader ReadBasicAuthHeader(ICommunicationContext context)
    {
      try
      {
        var header = context.Request.Headers["Authorization"];
        return string.IsNullOrEmpty(header) ? null : BasicAuthorizationHeader.Parse(header);
      }
      catch (ArgumentException)
      {
        return null;
      }
    }
  }
}