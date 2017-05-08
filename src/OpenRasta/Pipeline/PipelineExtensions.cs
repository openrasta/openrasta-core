using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Pipeline.CallGraph;

namespace OpenRasta.Pipeline
{
  public static class PipelineExtensions
  {
    static readonly Type[] RequiredKnownStages = typeof(KnownStages).GetNestedTypes();

    public static void VerifyKnownStagesRegistered(this IEnumerable<IPipelineContributor> contributors)
    {
      var missingTypes = RequiredKnownStages
        .Where(known => contributors.Where(known.IsInstanceOfType).Any() == false)
        .ToArray();
      if (missingTypes.Any())
      {
        throw new DependentContributorMissingException(missingTypes);
      }
    }

    public static void CheckPipelineInitialized(this IPipeline pipeline)
    {
#pragma warning disable 618
      if (!pipeline.IsInitialized)
        throw new InvalidOperationException("The pipeline has not been initialized and cannot run.");
#pragma warning restore 618
    }
  }
}