using System;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
  public class AsyncYielding
  {
    [Fact]
    public async Task middleware_yielding_same_thread()
    {
      var didItYield = await InvokeTillYield(
        new YieldBeforeNextMiddleware(nameof(YieldBeforeNextMiddleware)),
        new CodeMiddleware(() => Resumed = true));

      didItYield.ShouldBeTrue();
      Resumed.ShouldBeFalse();

      await Resume();

      Resumed.ShouldBeTrue();
    }

    [Fact]
    public async Task middleware_yielding_other_thread()
    {
      var didItYield = await InvokeTillYield(
        new OtherThreadMiddleware(),
        new YieldBeforeNextMiddleware(nameof(YieldBeforeNextMiddleware)),
        new CodeMiddleware(()=>Resumed = true)
        );


      didItYield.ShouldBeTrue();
      Resumed.ShouldBeFalse();

      await Resume();

      Resumed.ShouldBeTrue();
    }

    [Fact]
    public async Task middleware_yielding_before_code_on_other_thread()
    {
      var didItYield = await InvokeTillYield(
        new YieldBeforeNextMiddleware(nameof(YieldBeforeNextMiddleware)),
        new OtherThreadMiddleware(),
        new CodeMiddleware(()=>Resumed = true)
      );


      didItYield.ShouldBeTrue();
      Resumed.ShouldBeFalse();

      await Resume();

      Resumed.ShouldBeTrue();
    }

    [Fact]
    public async Task middleware_not_yielding_same_thread()
    {
      var didItYield = await InvokeTillYield(
        new BypassingCodeMiddleware(),
        new YieldBeforeNextMiddleware(nameof(YieldBeforeNextMiddleware)),
        new CodeMiddleware(()=>Resumed = true)
      );

      didItYield.ShouldBeFalse();
      Resumed.ShouldBeFalse();

      await Resume();

      Resumed.ShouldBeFalse();
    }

    [Fact]
    public async Task not_yielding_different_thread()
    {

      var didItYield = await InvokeTillYield(
        new OtherThreadMiddleware(),
        new BypassingCodeMiddleware(),
        new YieldBeforeNextMiddleware(nameof(YieldBeforeNextMiddleware)),
        new CodeMiddleware(()=>Resumed = true)
      );


      didItYield.ShouldBeFalse();
      Resumed.ShouldBeFalse();

      await Resume();

      Resumed.ShouldBeFalse();
    }


    ICommunicationContext Env;

    public AsyncYielding()
    {

      Env = new InMemoryCommunicationContext();
      
      Env.Yielder(nameof(YieldBeforeNextMiddleware), new TaskCompletionSource<bool>());
      Env.Resumer(nameof(YieldBeforeNextMiddleware), new TaskCompletionSource<bool>());
      Resumed = false;
    }
    bool Resumed { get; set; }
    
    async Task Resume()
    {
      Env.Resumer(nameof(YieldBeforeNextMiddleware)).SetResult(true);
      await Operation;
    }

    async Task<bool> InvokeTillYield(params IPipelineMiddlewareFactory[] factories)
    {
      Env.Yielder(nameof(YieldBeforeNextMiddleware), new TaskCompletionSource<bool>());
      Operation = factories.Compose().Invoke(Env);

      return await Yielding.DidItYield(
        Operation,
        Env.Yielder(nameof(YieldBeforeNextMiddleware)).Task);
    }

    Task Operation { get; set; }
  }

  class BypassingCodeMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    public Task Invoke(ICommunicationContext env)
    {
      return Task.FromResult(true);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return this;
    }
  }

  class OtherThreadMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    public Task Invoke(ICommunicationContext env)
    {
      return Task.Run(()=> Next.Invoke(env));
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }

    IPipelineMiddleware Next { get; set; }
  }
  class CodeMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly Action _action;

    public CodeMiddleware(Action action)
    {
      _action = action;
    }

    public Task Invoke(ICommunicationContext env)
    {
      _action();
      return Task.FromResult(true);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return this;
    }
  }
}