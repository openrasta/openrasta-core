using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public static class CommContextExtensions
  {
    public static TaskCompletionSource<bool> Yielder(this ICommunicationContext env, string name)
    {
      var key = $"openrasta.hosting.aspnet.yielders.{name}";
      return (TaskCompletionSource<bool>) env.PipelineData[key];
    }

    public static void Yielder(this ICommunicationContext env, string name,TaskCompletionSource<bool> source)
    {
      var key = $"openrasta.hosting.aspnet.yielders.{name}";
      env.PipelineData[key] = source;
    }
    public static TaskCompletionSource<bool> Resumer(this ICommunicationContext env, string name)
    {
      var key = $"openrasta.hosting.aspnet.resumers.{name}";

      return (TaskCompletionSource<bool>) env.PipelineData[key];
    } 
    public static void Resumer(this ICommunicationContext env, string name, TaskCompletionSource<bool> source)
    {
      var key = $"openrasta.hosting.aspnet.resumers.{name}";

      env.PipelineData[key] = source;
    }
  }
}