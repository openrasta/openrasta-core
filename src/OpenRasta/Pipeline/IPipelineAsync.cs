using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public interface IPipelineAsync
  {
    Task RunAsync(ICommunicationContext env);
  }
}