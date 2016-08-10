using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public interface IPipelineComponent
  {
    IPipelineComponent Build(IPipelineComponent next);
    Task Invoke(ICommunicationContext env);
  }

}
