using System.Linq;
using System.Net.NetworkInformation;
using OpenRasta.Pipeline;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class NetworkInterfaceContributor : KnownStages.IBegin
  {
    public void Initialize(IPipeline pipelineRunner)
    { 
      pipelineRunner.Notify(context =>
      {
        context.PipelineData["network.ipAddresses"] = NetworkInterface
          .GetAllNetworkInterfaces()
          .SelectMany(a => a.GetIPProperties().UnicastAddresses)
          .Select(a => a.Address)
          .ToList();
        return PipelineContinuation.Continue;
      });
    }
  }
}