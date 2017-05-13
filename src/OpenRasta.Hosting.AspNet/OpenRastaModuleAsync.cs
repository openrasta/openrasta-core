using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using OpenRasta.Concordia;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class OpenRastaModuleAsync : IHttpModule
  {
    public void Dispose()
    {
    }

    public static AspNetPipeline Pipeline => _pipeline.Value;
    public static ILogger Log = new TraceSourceLogger(new TraceSource("openrasta"));

    static readonly Lazy<AspNetPipeline> _pipeline = new Lazy<AspNetPipeline>(() =>
      HttpRuntime.UsingIntegratedPipeline
        ? (AspNetPipeline) new IntegratedPipeline()
        : new ClassicPipeline(), LazyThreadSafetyMode.PublicationOnly);

    PipelineStageAsync _postResolveStep;

    public void Init(HttpApplication context)
    {
      var startupProperties = new StartupProperties
      {
        OpenRasta =
        {
          Pipeline =
          {
            ContributorTrailers =
            {
              [call => call.Target is KnownStages.IUriMatching] =
              () => new YieldBeforeNextMiddleware(nameof(KnownStages.IUriMatching))
            }
          }
        }
      };
      var aspNetHost = new AspNetHost(startupProperties);
      Host = HostManager.RegisterHost(aspNetHost);
      aspNetHost.RaiseStart();

      _postResolveStep = new PipelineStageAsync(nameof(KnownStages.IUriMatching), aspNetHost, _pipeline);

      context.AddOnPostResolveRequestCacheAsync(_postResolveStep.Begin, _postResolveStep.End);
    }

    public HostManager Host { get; set; }
  }

  // A -> B -> Yield -> Resume -> C

  public static class Yielding
  {
    public static async Task<bool> DidItYield(Task pipeline, Task yielded)
    {
      if (pipeline.IsCompleted) return false;
      if (yielded.IsCompleted) return true;
      var completedTask = await Task.WhenAny(yielded, pipeline);
      return completedTask == yielded;
    }
  }


  public static class CommContextExtensions
  {
    public static TaskCompletionSource<bool> Yielder(this ICommunicationContext env, string name)
    {
      var key = $"openrasta.hosting.aspnet.yielders.{name}";
      if (env.PipelineData.ContainsKey(key) == false)
        env.PipelineData[key] = new TaskCompletionSource<bool>();
      return (TaskCompletionSource<bool>) env.PipelineData[key];
    }

    public static TaskCompletionSource<bool> Resumer(this ICommunicationContext env, string name)
    {
      var key = $"openrasta.hosting.aspnet.resumers.{name}";

      if (env.PipelineData.ContainsKey(key) == false)
        env.PipelineData[key] = new TaskCompletionSource<bool>();
      return (TaskCompletionSource<bool>) env.PipelineData[key];
    }
  }
}