using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public static class PipelineExtensions
  {
      static readonly Type[] RequiredKnownStages = typeof(KnownStages).GetNestedTypes();

      public static void Abort(this ICommunicationContext env)
    {
      env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;
      env.OperationResult = new OperationResult.InternalServerError
      {
        Title = "The request could not be processed because of a fatal error. See log below.",
        ResponseResource = env.ServerErrors
      };
      env.PipelineData.ResponseCodec = null;
      env.Response.Entity.Instance = env.ServerErrors;
      env.Response.Entity.Codec = null;
      env.Response.Entity.ContentLength = null;
    }

      public static void VerifyKnownStagesRegistered(this IEnumerable<IPipelineContributor> contributors)
      {
          var missingTypes = RequiredKnownStages
                  .Where(known => contributors.Where(known.IsInstanceOfType).Any() == false);
          if (missingTypes.Any())
          {
              throw new DependentContributorMissingException(missingTypes);
          }
      }
  }
}