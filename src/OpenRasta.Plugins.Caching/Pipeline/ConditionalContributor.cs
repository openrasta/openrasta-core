using System;
using System.Collections.Generic;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public abstract class ConditionalContributor
  {
    protected bool InvalidHeaderConbination(ICommunicationContext context)
    {
      return context.Request.InvalidConditionalHeaders(WarningErroneousCombination(context));
    }

    static Action<IEnumerable<string>> WarningErroneousCombination(ICommunicationContext context)
    {
      return _ => context.Response.AppendList(
        CachingHttpHeaders.WARNING,
        string.Format(
          "199 If-Lolcat: HTTP failed, the behavior of multiple conditionals ({0}) is undefined. The cats are laughing at you. Blame @blowdart.",
          _.JoinString(", ")));
    }

    protected static void NotModified(ICommunicationContext context)
    {
      context.Response.Entity.Instance = null;
      context.OperationResult = new OperationResult.NotModified();
      context.OperationResult.Execute(context);
    }
  }
}