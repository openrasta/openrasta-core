using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class DelegateMiddleware : IPipelineMiddleware
  {
    readonly Func<ICommunicationContext, Task> _invoke;

    public DelegateMiddleware(Func<ICommunicationContext, Task> invoke)
    {
      _invoke = invoke;
    }

    public Task Invoke(ICommunicationContext env) => _invoke(env);
  }
}