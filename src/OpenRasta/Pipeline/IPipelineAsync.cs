using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public interface IPipelineAsync
  {
    Task RunAsync(ICommunicationContext env);
    IEnumerable<IPipelineContributor> Contributors { get; }
    IEnumerable<IPipelineMiddlewareFactory> MiddlewareFactories { get; }
  }
}