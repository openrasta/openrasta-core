using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public interface IPipelineAsync
  {
    IEnumerable<IPipelineContributor> Contributors { get; }
    Task RunAsync(ICommunicationContext env);
  }
}