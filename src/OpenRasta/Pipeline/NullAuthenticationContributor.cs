namespace OpenRasta.Pipeline
{
  class NullAuthenticationContributor : NullPipelineContributor<
      KnownStages.IBegin,
      KnownStages.IHandlerSelection>,
    KnownStages.IAuthentication
  {}
}