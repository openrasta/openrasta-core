using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace Tests.Plugins.Caching.conditionals.if_modified_since
{
  abstract class HeaderSetter : IPipelineContributor
  {
    readonly string _header;
    readonly string _value;

    public HeaderSetter(string header, string value)
    {
      _header = header;
      _value = value;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(WriteLastModified)
        .Before<KnownStages.IOperationExecution>();
    }

    PipelineContinuation WriteLastModified(ICommunicationContext arg)
    {
      arg.Response.Headers[_header] = _value;
      return PipelineContinuation.Continue;
    }
  }
}