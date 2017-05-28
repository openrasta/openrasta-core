using System;
using System.Collections.Generic;
using OpenRasta.Authentication;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  [Obsolete]
  public class AuthenticationChallengerContributor : IPipelineContributor
  {
    readonly Func<IEnumerable<IAuthenticationScheme>> _schemes;

    public AuthenticationChallengerContributor(Func<IEnumerable<IAuthenticationScheme>> schemes)
    {
      _schemes = schemes;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ChallengeIfUnauthorized)
        .After<KnownStages.IOperationExecution>()
        .And
        .Before<KnownStages.IResponseCoding>();
    }

    private PipelineContinuation ChallengeIfUnauthorized(ICommunicationContext context)
    {
      if (!(context.OperationResult is OperationResult.Unauthorized))
        return PipelineContinuation.Continue;

      foreach (var scheme in _schemes())
        scheme.Challenge(context.Response);

      return PipelineContinuation.Continue;
    }
  }
}