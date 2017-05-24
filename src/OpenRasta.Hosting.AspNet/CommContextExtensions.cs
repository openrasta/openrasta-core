using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public static class CommContextExtensions
  {
    public static TaskCompletionSource<bool> Yielder(this ICommunicationContext env, string name)
    {
      var key = $"openrasta.hosting.aspnet.yielders.{name}";
      object val;
      if (!env.PipelineData.TryGetValue(key, out val))
        env.PipelineData[key] = val = new TaskCompletionSource<bool>();
      return (TaskCompletionSource<bool>) val;
    }

    public static TaskCompletionSource<bool> Resumer(this ICommunicationContext env, string name)
    {
      var key = $"openrasta.hosting.aspnet.resumers.{name}";

      object val;
      if (!env.PipelineData.TryGetValue(key, out val))
        env.PipelineData[key] = val = new TaskCompletionSource<bool>();
      return (TaskCompletionSource<bool>) val;
    }
  }
}