namespace OpenRasta.Pipeline
{
  class NullAuthenticationContributor : NullOrderedPipelineContributor<
      KnownStages.IBegin,
      KnownStages.IHandlerSelection>,
    KnownStages.IAuthentication
  {}
}