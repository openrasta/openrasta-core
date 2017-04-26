using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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

      var factories = Compose(InjectYields(LoadNamedFactories()));

      var postResolve = new PipelineStageAsync(YieldingStages[0], factories);

      context.AddOnPostResolveRequestCacheAsync(postResolve.Begin,postResolve.End);
      //context.AddOnEndRequestAsync();
    }

    IPipelineMiddleware Compose(IEnumerable<IPipelineMiddlewareFactory> factories)
    {

      factories = factories.Reverse().ToList();
      return factories.Skip(1).Aggregate(Middleware.Identity,
        (next, factory) => factory.Compose(next));
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
          yield return new YieldingMiddleware(factory.Key);

      }
    }
    public HostManager Host { get; set; }
  }
  class PipelineStageAsync
  {
    readonly string _yielderName;
    readonly IPipelineMiddleware _middleware;
    readonly EventHandlerTaskAsyncHelper _eventHandler;

    public PipelineStageAsync(string yielderName, IPipelineMiddleware middleware)
    {
      _yielderName = yielderName;
      _middleware = middleware;
      _eventHandler = new EventHandlerTaskAsyncHelper(Invoke);
    }

    async Task Invoke(object sender, EventArgs e)
    {
      var env = OpenRastaModule.CommunicationContext;
      await _middleware.Invoke(env);
      var notFound = env.OperationResult as OperationResult.NotFound;
      if (notFound?.Reason == NotFoundReason.NotMapped)
      {
        var tcs = env.Yielder(_yielderName);
        tcs.SetResult(false);
      }
      else
      {
        OpenRastaModuleAsync.Pipeline.HandoverToPipeline();
      }
    }

    public BeginEventHandler Begin => _eventHandler.BeginEventHandler;
    public EndEventHandler End => _eventHandler.EndEventHandler;
  }

  // A -> B -> Yield -> Resume -> C
  class YieldingMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly string _yieldName;

    public YieldingMiddleware(string yieldName)
    {
      _yieldName = yieldName;
    }
    public async Task Invoke(ICommunicationContext env)
    {
      var yielder = env.Yielder(_yieldName);
      await yielder.Task;
      await Next.Invoke(env);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }

    IPipelineMiddleware Next { get; set; }
  }

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
  static class CommContextExtensions
  {
    public static TaskCompletionSource<bool> Yielder(this ICommunicationContext env, string name)
    {
      return (TaskCompletionSource<bool>) env.PipelineData[$"openrasta.hosting.aspnet.yielders.{name}"];
    }

    public static TaskCompletionSource<bool> Resumer(this ICommunicationContext env, string name)
    {
      return (TaskCompletionSource<bool>) env.PipelineData[$"openrasta.hosting.aspnet.resumers.{name}"];
    }
  }
}