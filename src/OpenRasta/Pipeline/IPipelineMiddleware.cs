using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public interface IPipelineMiddleware
  {
    Task Invoke(ICommunicationContext env);
  }
}
