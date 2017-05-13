using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using OpenRasta.Concordia;
using OpenRasta.DI;
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

    static readonly Lazy<AspNetPipeline> _pipeline = new Lazy<AspNetPipeline>(() =>
      HttpRuntime.UsingIntegratedPipeline
        ? (AspNetPipeline) new IntegratedPipeline()
        : new ClassicPipeline(), LazyThreadSafetyMode.PublicationOnly);

    static readonly string[] YieldingStages = {nameof(KnownStages.IUriMatching)};

    public void Init(HttpApplication context)
    {
      Host = HostManager.RegisterHost(new AspNetHost());

      var initializer = Host.Resolver.Resolve<IPipelineInitializer>();

      initializer.Initialize(new StartupProperties
      {
        OpenRasta =
        {
          Pipeline = { ContributorTrailers =
          {
            [call=>call.Target is KnownStages.IUriMatching] = ()=> new YieldBeforeMiddleware(nameof(KnownStages.IUriMatching))
          }}
        }
      });
      var factories = InjectYields(LoadNamedFactories()).Compose();

      var postResolve = new PipelineStageAsync(YieldingStages[0], factories);

      context.AddOnPostResolveRequestCacheAsync(postResolve.Begin, postResolve.End);
      //context.AddOnEndRequestAsync();
    }

    IEnumerable<KeyValuePair<string, IPipelineMiddlewareFactory>> LoadNamedFactories()
    {
      throw new NotImplementedException();
    }

    IEnumerable<IPipelineMiddlewareFactory> InjectYields(
      IEnumerable<KeyValuePair<string, IPipelineMiddlewareFactory>> namedFactories)
    {
      foreach (var factory in namedFactories)
      {
        yield return factory.Value;
        if (YieldingStages.Contains(factory.Key))
          yield return new YieldBeforeMiddleware(factory.Key);
      }
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